#!/bin/bash -eux

dotnet run --project ./ml.net/InclusiveCodeReviews.Convert/InclusiveCodeReviews.Convert.csproj

cd onnxjs
if ! type npm >& /dev/null; then
    brew install npm
fi
if ! test -d node_modules; then
    npm install
fi
npm test
