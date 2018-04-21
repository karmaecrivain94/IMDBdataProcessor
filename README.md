# IMDBdataProcessor

This simple C# Console app takes in a `title.basics.tsv` file from imdb (see https://www.imdb.com/interfaces/) and outputs three different files:

* allgenres.tsv
* primarygenres.tsv
* runtimes.tsv

allgenres.tsv and primarygenres.tsv are formatted in the same way (years=columns, genres=rows) however the latter only counts the "primary" genre of a movie, meaning the first one as defined by IMDB. `allgenres.tsv` takes into account all three genres as defined by imdb

runtimes.tsv simply outputs a .tsv file with the average runtime per year of every movie in the dataset
