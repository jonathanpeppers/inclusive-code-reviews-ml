# CloudMine

If you would like to access CloudMine, see:

* https://1esdocs.azurewebsites.net/datainsights/cloudmine/access.html?tabs=user
* https://dataexplorer.azure.com/clusters/1es/databases/GitHub
* https://dataexplorer.azure.com/clusters/1es/databases/AzureDevOps

An example querying 100 random comments would be:

```kusto
PullRequestReviewComment
| where ReviewerLogin == 'rolfbjarne'
| where RepositoryName == 'xamarin-macios'
| where strlen(Body) > 10
| order by rand()
| limit 100
| project OrganizationLogin, RepositoryName, ReviewerLogin, HtmlUrl, Body
```

I would be careful and only grab from a public repository of a MSFT
employee that gave you permission.
