<?php
include('config.php');
include('classes.php');

if (!$debugMode) {
	chdir($cronDir);
}

ini_set('max_execution_time', 90);

if (!file_exists ($rawLocations['matrix']) && !file_exists ($rawLocations['drip'])) {
	die();
}

function findCloseCoordinates($pMatrixBordLocaties, $pLat, $pLon) {
	ksort($pMatrixBordLocaties);
	foreach ($pMatrixBordLocaties as $twoCoords => $existingMbLocatieArray) {
		$existingCoords = explode('_', $twoCoords);
		$existingLat = round(floatval($existingCoords[0]), 12);
		$existingLon = round(floatval($existingCoords[1]), 12);

		$maxLat = max($existingLat, $pLat);
		$maxLon = max($existingLon, $pLon);
		$minLat = min($existingLat, $pLat);
		$minLon = min($existingLon, $pLon);

		$difference = 0.00032;//0.00025;
		if ($maxLat-$minLat < $difference && $maxLon-$minLon < $difference) {/*
			$newLat = (($maxLat-$minLat)/2)+$minLat;
			$newLon = (($maxLon-$minLon)/2)+$minLon;
			$centerOfBothCoords = $newLat . '_' . $newLon;
			$pMatrixBordLocaties[$centerOfBothCoords] = $pMatrixBordLocaties[$twoCoords];
			unset($pMatrixBordLocaties[$twoCoords]);*/
			return [$twoCoords, $pMatrixBordLocaties];
		}
	}
	
	return [$pLat . '_' .  $pLon, $pMatrixBordLocaties];
}

$sLocationsJsonContents = file_get_contents($rawLocations['matrix']);
$oLocationsOfMatrixPortals = json_decode($sLocationsJsonContents);
$sLocationsJsonContents = null;
$locaties = $locatiesClassed = [];

foreach ($oLocationsOfMatrixPortals->features as $matrixBordLocatie) {
	$lat = round(floatval($matrixBordLocatie->geometry->coordinates[1]), 12);
	$lon = round(floatval($matrixBordLocatie->geometry->coordinates[0]), 12);

	$properties = $matrixBordLocatie->properties;
	$road = $properties->road;
	$side = $properties->carriagew0;
	$km = $properties->km;
	$location = $road . ' ' . $side . ' ' . $km;

	$coords = null;
	$returnValue = findCloseCoordinates($locaties, $lat, $lon);
	$coords = $returnValue[0];
	$locaties = $returnValue[1];
	if ($coords == null)
		$coords = $lat . '_' . $lon;
	
	
	$locaties [$coords][$location][$properties->uuid] = $properties->lane;

	asort($locaties [$coords][$location]);
	ksort($locaties [$coords]);
}

// DRIPlocations
$sDripLocationsXmlContents = file_get_contents($rawLocations['drip']);
$oDripLocationsData = simplexml_load_string(str_ireplace('soap:', '', $sDripLocationsXmlContents));
$vmsUnitRecords = $oDripLocationsData->Body->d2LogicalModel->payloadPublication->vmsUnitTable->vmsUnitRecord;

foreach ($vmsUnitRecords as $vmsUnitRecord) {
	$vmsRecord = $vmsUnitRecord->vmsRecord->vmsRecord;
	$locationForDisplay = $vmsRecord->vmsLocation->locationForDisplay;
	if (!$locationForDisplay)
		continue;

	$locationValue = (string)$vmsRecord->vmsDescription->values->value;
	$idArray = explode ('_', $vmsUnitRecord->attributes()->id);

	$coords = $locationForDisplay->latitude . '_' . $locationForDisplay->longitude;
	$location = explode(' ', $locationValue, 2)[1];
	$uuid = array_reverse($idArray)[0];

	$locaties [$coords][$location][$uuid] = 'DRIP';
}

$roadLocationArray = [];
foreach ($locaties as $coordinates => $locatie) {
	$roadLocation = new RoadLocation();
	
	$coordinate = new Coordinate();
	$coordinatesArray = explode ('_', $coordinates);
	$coordinate->lat = $coordinatesArray[0];
	$coordinate->lon = $coordinatesArray[1];
	$roadLocation->coordinate = $coordinate;
	
	$matrixPortal = new MatrixPortal();
	foreach ($locatie as $hmLocation => $lanes) {
		$roadWay = new RoadWay();
		$roadWay->hmLocation = $hmLocation;
		foreach ($lanes as $uuid => $number) {
			$lane = new Lane();
			$lane->uuid = $uuid;
			$lane->number = $number;
			$roadWay->lanes[] = $lane;
		}
		$matrixPortal->roadWays[] = $roadWay;
	}
	$roadLocation->matrixPortal = $matrixPortal;
	$roadLocationArray[] = $roadLocation;
}

file_put_contents($jsonMatrixPortaalLocatiesJsonFile, json_encode($roadLocationArray));
die();
