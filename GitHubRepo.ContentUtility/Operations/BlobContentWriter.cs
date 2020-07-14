// ------------------------------------------------------------------------------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------------------------------------------------------------------------------

using Octokit;
using System;
using System.Linq;
using System.Threading.Tasks;
using GitHubRepo.ContentUtility.Common;
using GitHubRepo.ContentUtility.Services;

namespace GitHubRepo.ContentUtility.Operations
{
    /// <summary>
    /// Provides methods for writing contents to a specific blob in a GitHub repository.
    /// </summary>
    public class BlobContentWriter
    {
        /// <summary>
        /// Writes contents to a specified blob in the specified GitHub repository.
        /// </summary>
        /// <param name="appConfig">The application configuration object which contains values
        /// for connecting to the specified GitHub repository.</param>
        /// <param name="privateKey"> The RSA private key of a registered GitHub app installed in the specified repository.</param>
        /// <returns>A task.</returns>
        public async Task WriteToRepositoryAsync(ApplicationConfig appConfig, string privateKey)
        {
            if (appConfig == null)
            {
                throw new ArgumentNullException(nameof(appConfig), "Parameter cannot be null");
            }

            try
            {
                string token = await GitHubAuthService.GetGithubAppTokenAsync(appConfig, privateKey);

                // Pass the JWT as a Bearer token to Octokit.net
                var finalClient = new GitHubClient(new ProductHeaderValue(appConfig.GitHubAppName))
                {
                    Credentials = new Credentials(token, AuthenticationType.Bearer)
                };

                // Get repo references
                var references = await finalClient.Git.Reference.GetAll(appConfig.GitHubOrganization, appConfig.GitHubRepoName);

                // Check if the working branch is in the refs
                var workingBranch = references.Where(reference => reference.Ref == $"refs/heads/{appConfig.WorkingBranch}").FirstOrDefault();

                // Check if branch already exists.
                if (workingBranch == null)
                {
                    // Working branch does not exist so branch off reference branch
                    var refBranch = references.Where(reference => reference.Ref == $"refs/heads/{appConfig.ReferenceBranch}").FirstOrDefault();

                    // Exception will throw if branch already exists
                    var newBranch = await finalClient.Git.Reference.Create(appConfig.GitHubOrganization, appConfig.GitHubRepoName,
                        new NewReference($"refs/heads/{appConfig.WorkingBranch}", refBranch.Object.Sha));

                    // create file
                    var createChangeSet = await finalClient.Repository.Content.CreateFile(
                        appConfig.GitHubOrganization,
                        appConfig.GitHubRepoName,
                        appConfig.FileContentPath,
                        new CreateFileRequest(appConfig.CommitMessage,
                            appConfig.FileContent,
                            appConfig.WorkingBranch));
                }
                else
                {
                    // Get reference of the working branch
                    var masterReference = await finalClient.Git.Reference.Get(appConfig.GitHubOrganization,
                        appConfig.GitHubRepoName, workingBranch.Ref);

                    // Get the latest commit of this branch
                    var latestCommit = await finalClient.Git.Commit.Get(appConfig.GitHubOrganization, appConfig.GitHubRepoName,
                        masterReference.Object.Sha);

                    // Create blob
                    NewBlob blob = new NewBlob { Encoding = EncodingType.Utf8, Content = appConfig.FileContent };
                    BlobReference blobRef =
                        await finalClient.Git.Blob.Create(appConfig.GitHubOrganization, appConfig.GitHubRepoName, blob);

                    // Create new Tree
                    var tree = new NewTree { BaseTree = latestCommit.Tree.Sha };

                    // Add items based on blobs
                    tree.Tree.Add(new NewTreeItem
                    {
                        Path = appConfig.FileContentPath, Mode = appConfig.TreeItemMode.ToString(), Type = TreeType.Blob, Sha = blobRef.Sha
                    });

                    var newTree = await finalClient.Git.Tree.Create(appConfig.GitHubOrganization, appConfig.GitHubRepoName, tree);

                    // Create commit
                    var newCommit = new NewCommit(appConfig.CommitMessage, newTree.Sha,
                        masterReference.Object.Sha);
                    var commit =
                        await finalClient.Git.Commit.Create(appConfig.GitHubOrganization, appConfig.GitHubRepoName, newCommit);

                    // Push the commit
                    await finalClient.Git.Reference.Update(appConfig.GitHubOrganization, appConfig.GitHubRepoName, workingBranch.Ref,
                        new ReferenceUpdate(commit.Sha));
                }

                // Create PR
                var pullRequest = await finalClient.Repository.PullRequest.Create(appConfig.GitHubOrganization,
                    appConfig.GitHubAppName,
                    new NewPullRequest(appConfig.PullRequestTitle, appConfig.WorkingBranch, appConfig.ReferenceBranch) { Body = appConfig.PullRequestBody });

                // Add reviewers
                var teamMembers = appConfig.Reviewers;
                var reviewersResult = await finalClient.Repository.PullRequest.ReviewRequest.Create(appConfig.GitHubOrganization,
                    appConfig.GitHubRepoName, pullRequest.Number,
                    new PullRequestReviewRequest(teamMembers.AsReadOnly(), null));

                // Add label
                var issueUpdate = new IssueUpdate();
                issueUpdate.AddAssignee(appConfig.PullRequestAssignee);
                issueUpdate.AddLabel(appConfig.PullRequestLabel);

                // Update the PR with the relevant info
                await finalClient.Issue.Update(appConfig.GitHubOrganization, appConfig.GitHubRepoName, pullRequest.Number,
                    issueUpdate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
