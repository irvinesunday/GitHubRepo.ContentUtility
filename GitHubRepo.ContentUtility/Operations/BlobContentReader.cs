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
    /// Provides methods for reading the contents of a specific blob in a GitHub repository.
    /// </summary>
    public class BlobContentReader
    {
        /// <summary>
        /// Reads the contents of a specified blob in the specified GitHub repository.
        /// </summary>
        /// <param name="appConfig">The application configuration object which contains values
        /// for connecting to the specified GitHub repository.</param>
        /// <param name="privateKey"> The RSA private key of a registered GitHub app installed in the specified repository.</param>
        /// <returns></returns>
        public async Task<string> ReadRepositoryBlobContentAsync(ApplicationConfig appConfig, string privateKey)
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

                // Check if the reference branch is in the refs
                var referenceBranch = references.Where(reference => reference.Ref == $"refs/heads/{appConfig.ReferenceBranch}").FirstOrDefault();

                if (referenceBranch == null)
                {
                    throw new ArgumentException(nameof(appConfig.ReferenceBranch), "Branch doesn't exist in the repository");
                }

                var fileContents = await finalClient.Repository.Content.GetAllContents(
                       appConfig.GitHubOrganization,
                       appConfig.GitHubRepoName,
                       appConfig.FileContentPath);

                if (fileContents.Any())
                {
                    return fileContents[0].Content;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
