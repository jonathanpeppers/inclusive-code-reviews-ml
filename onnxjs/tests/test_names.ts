import { expect } from 'chai';
import * as ort from 'onnxruntime-node';

describe('name tests', async () => {
    const session = await ort.InferenceSession.create('../bin/model.onnx');
    expect(session).to.be.not.null;

    const githubHandleRegex:RegExp = /\B@([a-z0-9](?:-(?=[a-z0-9])|[a-z0-9]){0,38}(?<=[a-z0-9]))/gi;
    const backtickRegex:RegExp = /`+[^`]+`+/gi;
    const urlRegex:RegExp = /\b(https?|ftp|file):\/\/[\-A-Za-z0-9+&@#\/%?=~_|!:,.;]*[\-A-Za-z0-9+&@#\/%=~_|]/gi;
    const punctuationRegex:RegExp = /(\.|\!|\?|;|:)+$/g;

    async function testText(text:string) {
        // Apply the same processing as the app does to text
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
        return { result, score };
    }
    
    it('should handle names correctly', async () => {
        // Test various names
        const names = [
            "Peter", 
            "His literal name is just Peter!",
            "Piotr",
            "This is Peter.",
            "Here comes Piotr."
        ];
        
        for (const name of names) {
            const { result } = await testText(name);
            expect(result).to.be.equal("0"); // Should be not negative
        }
    });
});