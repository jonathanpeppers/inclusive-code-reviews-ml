if (!(Test-Path "node_modules")) { 
    echo "node_modules does not exist, restore packages ..."
    npm install
 }

echo "generate model"
dotnet run --project ..\ml.net\InclusiveCodeReviews.Convert\InclusiveCodeReviews.Convert.csproj

echo "Run tests"
npm test
