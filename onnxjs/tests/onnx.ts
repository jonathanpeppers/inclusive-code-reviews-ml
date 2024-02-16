import { expect } from 'chai';
import * as ort from 'onnxruntime-node';
import test_cases from './test_cases.json';

describe('onnx tests', async () => {
    const session = await ort.InferenceSession.create('../bin/model.onnx');
    expect(session).to.be.not.null;

    const githubHandleRegex:RegExp = /\B@([a-z0-9](?:-(?=[a-z0-9])|[a-z0-9]){0,38}(?<=[a-z0-9]))/gi;
    const backtickRegex:RegExp = /`+[^`]+`+/gi;
    const urlRegex:RegExp = /\b(https?|ftp|file):\/\/[\-A-Za-z0-9+&@#\/%?=~_|!:,.;]*[\-A-Za-z0-9+&@#\/%=~_|]/gi;
    const punctuationRegex:RegExp = /(\.|!|\?|;|:)+$/g;

    async function assertText(
        text:string,
        isnegative:string,
        isNegativeConfidence:number,
        isPositiveConfidence:number,
    ) {
        const github_replaced = text.replace(githubHandleRegex, '@github');
        const backtick_replaced = github_replaced.replace(backtickRegex, '#code');
        const urls_replaced = backtick_replaced.replace(urlRegex, '#url');
        const punctuation_replaced = urls_replaced.replace(punctuationRegex, '');
        const results = await session.run({
            text: new ort.Tensor([punctuation_replaced.trim()], [1,1]),
            isnegative: new ort.Tensor([''], [1,1]),
            importance: new ort.Tensor('float32', [''], [1,1]),
        })
        expect(results).to.be.not.null;

        const result = results['PredictedLabel.output'].data[0];
        const score = results['Score.output'].data[Number(result)];
        console.log(`Text '${text}', IsNegative ${result}, Confidence ${score}`);
        expect(isnegative).to.be.equal(result);
        expect(score).to.be.greaterThan(isnegative == "1" ? isNegativeConfidence : isPositiveConfidence);
    }

    test_cases.forEach(x => {
        it(x.text, async () => {
            // NOTE: for test_cases.json
            // We want to be 70% confident for cases where isnegative is 1
            // We only need to be 50% confident for cases where isnegative is 0
            await assertText(x.text, x.isnegative, 0.7, 0.5);
        });
    })
});