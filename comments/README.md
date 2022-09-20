# CSV Files

* `classified.csv` - the main training set our model is trained against
* `potential-bad-*.csv` - *real* code review comments from public GitHub repositories. They *might* have bad comments, as we used queries containing specific words.
* `test.csv` - used merely for testing the MLTrainer app
* `toxic-comments.csv` - based off [this data set][kaggle]. This is a collection of *very* rude or toxic comments from Wikipedia change reviews. Parental discretion advised!

[kaggle]: https://www.kaggle.com/datasets/fizzbuzz/cleaned-toxic-comments
