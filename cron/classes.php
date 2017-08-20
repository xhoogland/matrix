<?php
class RoadLocation {
	public $coordinate; // type: Coordinate
	
	public $matrixPortal; // type: MatrixPortal
}

class Coordinate {
	public $lat; // type: float
	
	public $lon; // type: float
}

class MatrixPortal {
	public $roadWays; // type: array (of RoadWay)
}

class RoadWay {
	public $hmLocation; // type: string
	
	public $lanes; // type: array (of Lane)
}

class VmsInfo {
	public $uuid; // type: string	
	
	public $shownSign; // type: string
}

class Lane extends VmsInfo {
	public $number; // type: int
}
