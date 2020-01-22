
define([
	'qvangular',
	'underscore',
	'QlikSimpleConnector.webroot/connect/connect-datamanager',
	'QlikSimpleConnector.webroot/select/select-datamanager',
],
function ( qvangular, _, connect, select, info ) {

	var connectionInfoResult = null;

	function create(serverside) {
		
		function getInitialSelectStep() {
			return select;
		}
		
		function generateSelectScript() {
			console.log('generateSelectScript');
		}
		
		function getRawScript(connectionServerside, table) {
			console.log('getRawScript', connectionServerside, table);
			
			return new Promise(function(resolve, reject) {
				resolve(
`LOAD
${table.fields.map((field) => `${field.name} AS ${field.alias}`).join(',\n')}
;
SQL SELECT
${table.fields.map((field) => `${field.name}`).join(',\n')}
FROM
	${table.databaseName}.${table.ownerName}.${table.tableName};
`
				);
			});
		}

		var entrypoints = [];

		entrypoints.push({
			id: "simple",
			category: "database",
			name: "Simple",
			imagePath: "../resources/../customdata/64/QlikSimpleConnector/web/simple.png",
			imageSquarePath: "../resources/../customdata/64/QlikSimpleConnector/web/simple.png",
			initialConnectStep: connect
		});

		var connector = {
			entrypoints: entrypoints,
			getInitialSelectStep: getInitialSelectStep,
			generateSelectScript: generateSelectScript,
			getRawScript: getRawScript
		};
		
		return connector;

	}

	var exports = {
		create: create
	};

	return exports;
});