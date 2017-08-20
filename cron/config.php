<?php
// Path to the directory where matrixdata.php is placed in, all the paths under here have the path in the variable as 'basepath'. It is recommended to keep this out of the public web so it is not reachable
$cronDir = '/home/matrix/cron/';

// Path where the downloaded live info is placed
$packedFile = 'packed/Matrixsignaalinformatie.xml.gz';

// Path where the unpacked live info is placed
$xmlFile = 'unpack/Matrixsignaalinformatie.xml';

// Path of the geojson-file with locations of the variable message sings
$jsonMatrixLocatiesJsonFile = 'matrixLocaties.json';

// Path of the generated json file with locations of the variable message signs used in the frontend at first pageload
$jsonMatrixPortaalLocatiesJsonFile = '../html/static/matrixPortaalLocaties.json';

// Path to the generated json with key-value pairs of respectively the guid of a sign and what it currently shows
$jsonMatrixBordenOutputPath = '../html/live/matrixBorden.json';

// Url to the location where the live info is made available
$openDataUrl = 'http://opendata.ndw.nu/Matrixsignaalinformatie.xml.gz';

// Mostly when developing a newer version in a subfolder of the root. When placing the files of $cronDir over there, edit the paths above this line and change this variable to true there is no live data downloaded for the testversion, but just uses an already downloaded version
$debugMode = false;
