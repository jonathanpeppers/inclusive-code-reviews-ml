// Test script to verify "Your helpfulness!" classification
const ort = require('onnxruntime-node');

async function testHelpfulness() {
    const session = await ort.InferenceSession.create('../bin/model.onnx');
    const githubHandleRegex = /\B@([a-z0-9](?:-(?=[a-z0-9])|[a-z0-9]){0,38}(?<=[a-z0-9]))/gi;
    const backtickRegex = /`+[^`]+`+/gi;
    const urlRegex = /\b(https?|ftp|file):\/\/[\-A-Za-z0-9+\&@#\/%?=~_|!:,.;]*[\-A-Za-z0-9+\&@#\/%=~_|]/gi;
    const punctuationRegex = /(\.|\!|\?|;|:)+$/g;
    
    const text = "Your helpfulness!";
    const github_replaced = text.replace(githubHandleRegex, '@github');
    const backtick_replaced = github_replaced.replace(backtickRegex, '#code');
    const urls_replaced = backtick_replaced.replace(urlRegex, '#url');
    const punctuation_replaced = urls_replaced.replace(punctuationRegex, '');
    
    const results = await session.run({
        text: new ort.Tensor([punctuation_replaced.trim()], [1,1]),
        isnegative: new ort.Tensor([''], [1,1]),
        importance: new ort.Tensor('float32', [''], [1,1]),
    });
    
    const result = results['PredictedLabel.output'].data[0];
    const score = results['Score.output'].data[Number(result)];
    console.log(`Text '${text}', IsNegative ${result}, Confidence ${score}`);
    
    if (result === '0') {
        console.log("PASS: The phrase is correctly classified as positive/neutral");
    } else {
        console.log("FAIL: The phrase is incorrectly classified as negative");
        process.exit(1);
    }
}

testHelpfulness().catch(console.error);
