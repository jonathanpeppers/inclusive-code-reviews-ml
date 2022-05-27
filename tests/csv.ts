import * as tf from '@tensorflow/tfjs';
import { expect } from 'chai';

describe('csv tests', () => {
    it('can load a csv file', async () => {
        // Code came from: https://js.tensorflow.org/api/latest/#Data
        const csvUrl = 'https://storage.googleapis.com/tfjs-examples/multivariate-linear-regression/data/boston-housing-train.csv';
        const csvDataset = tf.data.csv(csvUrl, {
            columnConfigs: {
                medv: {
                    isLabel: true
                }
            }
        });

        // Number of features is the number of column names minus one for the label
        // column.
        const numOfFeatures = (await csvDataset.columnNames()).length - 1;

        // Prepare the Dataset for training.
        const flattenedDataset = csvDataset.map((val: any) => {
            const {xs, ys} = val; // can cast xs and ys to appropriate type
            // Convert xs(features) and ys(labels) from object form (keyed by
            // column name) to array form.
            return {xs:Object.values(xs), ys:Object.values(ys)};
        })
        .batch(10);

        // Define the model.
        const model = tf.sequential();
        model.add(tf.layers.dense({
            inputShape: [numOfFeatures],
            units: 1
        }));
        model.compile({
            optimizer: tf.train.sgd(0.000001),
            loss: 'meanSquaredError'
        });

        // Fit the model using the prepared Dataset
        var fitted = model.fitDataset(flattenedDataset, {
            epochs: 10,
            callbacks: {
                onEpochEnd: async (epoch, logs) => {
                    console.log(epoch + ':' + logs.loss);
                }
            }
        });
        expect(fitted).to.be.not.null;
    });
});