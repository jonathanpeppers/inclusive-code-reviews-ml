param
(
    [string] $name = 'InclusiveCodeReviews',
    [int] $seconds = 10
)

$ErrorActionPreference = 'Stop'
& dotnet tool update --global mlnet
& mlnet classification --dataset comments/classified.csv --label-col 1 --has-header true --train-time $seconds --name $name
Remove-Item ml.net -Recurse -Force -ErrorAction Continue
Rename-Item $name ml.net
