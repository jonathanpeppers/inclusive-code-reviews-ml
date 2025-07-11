import * as ort from 'onnxruntime-node';

async function testPhrase() {
    const session = await ort.InferenceSession.create('../bin/model.onnx');
    
    const testText = 'Is the answer "don\'t do that"?';
    
    // Apply same preprocessing as in the test
    const githubHandleRegex = /\B@([a-z0-9](?:-(?=[a-z0-9])|[a-z0-9]){0,38}(?<=[a-z0-9]))/gi;
    const backtickRegex = /`+[^`]+`+/gi;
    const urlRegex = /\b(https?|ftp|file):\/\/[\-A-Za-z0-9+&@#\/%?=~_|!:,.;]*[\-A-Za-z0-9+&@#\/%=~_|]/gi;
    const punctuationRegex = /(\.|!|\?|;|:)+$/g;
    
    const github_replaced = testText.replace(githubHandleRegex, '@github');
    const backtick_replaced = github_replaced.replace(backtickRegex, '#code');
    const urls_replaced = backtick_replaced.replace(urlRegex, '#url');
    const punctuation_replaced = urls_replaced.replace(punctuationRegex, '');
    
    console.log(`Original: "${testText}"`);
    console.log(`Preprocessed: "${punctuation_replaced.trim()}"`);
    
    const results = await session.run({
        text: new ort.Tensor([punctuation_replaced.trim()], [1,1]),
        isnegative: new ort.Tensor([''], [1,1]),
        importance: new ort.Tensor('float32', [''], [1,1]),
    });
    
    const result = results['PredictedLabel.output'].data[0];
    const score = results['Score.output'].data[Number(result)];
    
    console.log(`Prediction: IsNegative ${result}, Confidence ${score}`);
    if (result == "1") {
        console.log("❌ PROBLEM: This phrase is being classified as negative when it should be positive");
    } else {
        console.log("✅ OK: This phrase is correctly classified as positive");
    }
}

testPhrase().catch(console.error);