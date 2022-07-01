import { expect } from 'chai';
import * as ort from 'onnxruntime-node';

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

    var names = [
        {
            text: 'This is terrible',
            isnegative: '1',
        },
        {
            text: 'You suck',
            isnegative: '1',
        },
        {
            text: 'This is great!',
            isnegative: '0',
        },
    ];

    names.forEach(x => {
        it(x.text, async () => {
            await assertText(x.text, x.isnegative, 0.7);
        });
    })
});