import { expect } from 'chai';
import * as ort from 'onnxruntime-node';
import test_cases from './test_cases.json';

describe('onnx tests', async () => {
    const session = await ort.InferenceSession.create('./model.onnx');
    expect(session).to.be.not.null;

    async function assertText(text:string, isnegative:string, confidence:number) {
        const results = await session.run({
            text: new ort.Tensor([text], [1,1]),
            isnegative: new ort.Tensor([''], [1,1]),
        })
        expect(results).to.be.not.null;

        const result = results['PredictedLabel.output'].data[0];
        const score = results['Score.output'].data[Number(result)];
        console.log(`Text '${text}', IsNegative ${result}, Confidence ${score}`);
        expect(result).to.be.equal(isnegative);
        expect(score).to.be.greaterThan(confidence);
    }

    test_cases.forEach(x => {
        it(x.text, async () => {
            await assertText(x.text, x.isnegative, 0.7);
        });
    })
});