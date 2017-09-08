<?php
include('config.php');
include('classes.php');

function unpackGzFile($pOpenDataUrl, $pPackedFile, $pXmlFile) {
	file_put_contents($pPackedFile, file_get_contents($pOpenDataUrl));
	
	$bufferSize = 1024;
	$inXmlGzFile = gzopen($pPackedFile, 'rb');
	$outXmlFile = fopen($pXmlFile, 'wb');

	while (!gzeof($inXmlGzFile)) {
		fwrite($outXmlFile, gzread($inXmlGzFile, $bufferSize));
	}

	fclose($outXmlFile);
	gzclose($inXmlGzFile);
	$outXmlFile = $inXmlGzFile = null;
}

if (!$debugMode) {
	chdir($cronDir);

	unpackGzFile($openDataUrl['matrix'], $packedFile['matrix'], $xmlFile['matrix']);
	unpackGzFile($openDataUrl['drip'], $packedFile['drip'], $xmlFile['drip']);
}

//if (!file_exists ($jsonMatrixPortaalLocatiesJsonFile)) {
//	die();
//}

function getImageNameForSignShown ($display) {
	$imageName = 'onbekend';
	$tag = null;
	// built in flashers...
	if (isset ($display->restriction_end))
	{
		$tag = $display->restriction_end;
		$imageName = 'einderestrictie';
	}
	elseif (isset ($display->lane_open))
	{
		$tag = $display->lane_open;
		$imageName = 'groenepijl';
	}
	elseif (isset ($display->blank))
	{
		$tag = $display->blank;	
		$imageName = 'leeg';
	}
	elseif (isset ($display->lane_closed))
	{
		$tag = $display->lane_closed;
		$imageName = 'roodkruis';
	}
	elseif (isset ($display->speedlimit))
	{
		$tag = $display->speedlimit;
		$imageName = 'snelheid_' . $display->speedlimit;
	}
	elseif (isset ($display->lane_closed_ahead))
	{
		$tag = $display->lane_closed_ahead;
		$imageName = 'verdrijfpijl_';
		if (isset ($display->lane_closed_ahead->merge_left))
		{
			$imageName = $imageName . 'links';
		}
		else
		{
			$imageName = $imageName . 'rechts';
		}
	}
	
	return $imageName;
}

function getDataUrlForDripShown ($imageData) {
	$mimeType = $imageData->mimeType;
	$encoding = $imageData->encoding;
	$binary = $imageData->binary;

	return 'data:' . $mimeType . ';' . $encoding . ',' . $binary;// . '==';
}

$sLiveXmlContents = file_get_contents($xmlFile['matrix']);
$oLiveMatrixData = simplexml_load_string(str_ireplace('ndw:', '', str_ireplace('soap:', '', $sLiveXmlContents)));
$events = $oLiveMatrixData->Body->NdwVms->variable_message_sign_events->event;

$signs = [];
foreach ($events as $matrixBord) {
	if (!isset($matrixBord->display))
		continue;

	$vmsInfo = new VmsInfo();
	$vmsInfo->uuid = (string)$matrixBord->sign_id->uuid;
	$vmsInfo->shownSign = getImageNameForSignShown($matrixBord->display);
	
	$signs[] = $vmsInfo;
}
$sLiveXmlContents = $oLiveMatrixData = $events = null;

$sLiveXmlContents = file_get_contents($xmlFile['drip']);
$oLiveDripData = simplexml_load_string(str_ireplace('soap:', '', $sLiveXmlContents));
$vmsUnits = $oLiveDripData->Body->d2LogicalModel->payloadPublication->vmsUnit;

foreach ($vmsUnits as $vmsUnit) {
	$vmsMessageExtension = $vmsUnit->vms->vms->vmsMessage->vmsMessage->vmsMessageExtension;
	if (!isset($vmsMessageExtension))
		continue;

	$idArray = explode ('_', $vmsUnit->vmsUnitReference->attributes()->id);
	$uuid = array_reverse($idArray)[0];

	$vmsInfo = new VmsInfo();
	$vmsInfo->uuid = $uuid;
	$vmsInfo->shownSign = getDataUrlForDripShown($vmsMessageExtension->vmsMessageExtension->vmsImage->imageData);

	$signs[] = $vmsInfo;
}
$sLiveXmlContents = $oLiveDripData = $vmsUnits = null;

file_put_contents($jsonMatrixBordenOutputPath, json_encode($signs));
die();
