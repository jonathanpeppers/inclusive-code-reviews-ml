import { expect } from 'chai';
import * as ort from 'onnxruntime-node';

describe('onnx tests', () => {
    it('can load a onnx file', async () => {
        const session = await ort.InferenceSession.create('./model.onnx');
        expect(session).to.be.not.null;
    });
});