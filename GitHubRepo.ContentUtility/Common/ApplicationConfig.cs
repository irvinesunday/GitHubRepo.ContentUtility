﻿// ------------------------------------------------------------------------------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using static GitHubRepo.ContentUtility.Common.Enums;

namespace GitHubRepo.ContentUtility.Common
{
    /// <summary>
    /// Specifies the common property values required to connect to a specific GitHub repository.
    /// </summary>
    public class ApplicationConfig
    {
        /// <summary>
        /// The name of the GitHub app
        /// </summary>
        public string GitHubAppName { get; set; }

        /// <summary>
        /// The owner of the GitHub repository
        /// </summary>
        public string GitHubOrganization { get; set; }

        /// <summary>
        /// The name of the GitHub repository
        /// </summary>
        public string GitHubRepoName { get; set; }

        /// <summary>
        /// The GitHub app Id
        /// </summary>
        public int GitHubAppId { get; set; }

        /// <summary>
        /// The remote branch where commits are made into
        /// </summary>
        public string WorkingBranch { get; set; }

        /// <summary>
        /// The remote branch which is the base reference of the <see cref="WorkingBranch"/>
        /// </summary>
        public string ReferenceBranch { get; set; }

        /// <summary>
        /// The file path of a file content in a repository branch
        /// </summary>
        public string FileContentPath { get; set; }

        /// <summary>
        /// The string contents of a file in a respository branch
        /// </summary>
        public string FileContent { get; set; }

        /// <summary>
        /// The commit message
        /// </summary>
        public string CommitMessage { get; set; }

        /// <summary>
        /// Pull request title
        /// </summary>
        public string PullRequestTitle { get; set; }

        /// <summary>
        /// Pull request message
        /// </summary>
        public string PullRequestBody { get; set; }

        /// <summary>
        /// List of pull request reviewers
        /// </summary>
        public List<string> Reviewers { get; set; }

        /// <summary>
        /// Pull request assignee
        /// </summary>
        public string PullRequestAssignee { get; set; }

        /// <summary>
        /// Pull request label
        /// </summary>
        public string PullRequestLabel { get; set; }

        /// <summary>
        /// The mode of the file being written to the repository
        /// </summary>
        public TreeItemMode TreeItemMode { get; set; } = TreeItemMode.Blob;
    }
}
