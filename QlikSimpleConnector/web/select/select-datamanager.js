define([
	'qvangular',
	'underscore',
	'text!QlikSimpleConnector.webroot/select/select-datamanager.html',
	'css!QlikSimpleConnector.webroot/select/select-datamanager.css',
],
function (qvangular, _, template, css) {

	return {
		template: template,
		scope: {
			qvDialogContext: '=',
			dataconnectionSelectionEditor: '=',
			dataconnectorInstance: '=',
			qvInternals: '=',
			tableModelToEdit: '=',
			serverside: '=',
			standardSelectDialogService: '='
		},

		controller: ['$scope', 'input', '$q', 'serverside', function ($scope, input, $q, serverside) {

			function getDatabases() {
                return serverside.sendJsonRequest("getDatabases", [{
                    paramName: 'qConnectionId',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: $scope.dataconnectorInstance.id, Value: null }]
                }]).then(function (response) {
                    return response.qDatabases;
                });
            }
			
			function getOwners(qDatabaseName) {
                return serverside.sendJsonRequest("getOwners", [{
                    paramName: 'qConnectionId',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: $scope.dataconnectorInstance.id, Value: null }]
                }].concat([{
                    paramName: 'qDatabase',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qDatabaseName, Value: null }]
                }])).then(function (response) {
                    return response.qOwners.map((owner) => ({
						qName: owner,
					}));
                });
            }
			
			function getTables(qDatabaseName, qOwnerName) {
                return serverside.sendJsonRequest("getTables", [{
                    paramName: 'qConnectionId',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: $scope.dataconnectorInstance.id, Value: null }]
                }].concat([{
                    paramName: 'qDatabase',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qDatabaseName, Value: null }]
                }]).concat([{
                    paramName: 'qOwner',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qOwnerName, Value: null }]
                }])).then(function (response) {
                    return response.qTables.map((table) => ({
						...table,
						checked: false,
						indeterminate: false,
					}));
                });
            }
			
			function getFields(qDatabaseName, qOwnerName, qTableName) {
                return serverside.sendJsonRequest("getFields", [{
                    paramName: 'qConnectionId',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: $scope.dataconnectorInstance.id, Value: null }]
                }].concat([{
                    paramName: 'qDatabase',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qDatabaseName, Value: null }]
                }]).concat([{
                    paramName: 'qOwner',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qOwnerName, Value: null }]
                }]).concat([{
                    paramName: 'qTable',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qTableName, Value: null }]
                }])).then(function (response) {
                    return response.qFields.map((field) => ({
						...field,
						checked: false,
					}));
                });
            }
			
			function getPreview(qDatabaseName, qOwnerName, qTableName) {
                return serverside.sendJsonRequest("getPreview", [{
                    paramName: 'qConnectionId',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: $scope.dataconnectorInstance.id, Value: null }]
                }].concat([{
                    paramName: 'qDatabase',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qDatabaseName, Value: null }]
                }]).concat([{
                    paramName: 'qOwner',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qOwnerName, Value: null }]
                }]).concat([{
                    paramName: 'qTable',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qTableName, Value: null }]
                }])).then(function (response) {
                    return response.qPreview.slice(1);
                });
            }
			
			function _onTableShow(table) {
				if ($scope.selectedTable !== table) {
					$scope.selectedTable = table;

					$scope.fieldsProm = getFields($scope.databaseInput.qName, $scope.ownerInput.qName, table.qName)
						.then((fields) => {
							fields.forEach((field) => field.table = table);
							table.fields = fields;
							return fields;
						})
						
					$scope.previewProm = getPreview($scope.databaseInput.qName, $scope.ownerInput.qName, table.qName)
						.then((preview) => {
							table.preview = preview;
							return preview;
						})

					return $q.all([$scope.fieldsProm, $scope.previewProm]);
				} else {
					return $q.all([$scope.fieldsProm, $scope.previewProm]);
				}
			}
			
			function buildLoadModel() {
				
				$scope.tables.filter((table) => table.checked || table.indeterminate).forEach((table) => {
					const tableModel = $scope.dataconnectionSelectionEditor.getOrCreateTableModelByDatabasePath($scope.databaseInput.qName, $scope.ownerInput.qName, table.qName);
					
					tableModel.data.tableKey = 'toto';
					
					if(tableModel.data.fields.length > 0) {
						while(tableModel.data.fields.length > 0) {
							tableModel.data.fields.pop();
						}
					}
					
					table.fields.forEach((field) => {
						tableModel.data.fields.push({ alias: field.qName, name: field.qName, selected : field.checked });
					})
					
					tableModel.data.loadProperties = {
						filterType: 0,
						filterClause: ''
					};
					
					console.log(tableModel);
					
				})
			}
			
			$scope.baseNext = $scope.qvDialogContext.next;

			$scope.qvDialogContext.next = function () {
				buildLoadModel();
				if($scope.baseNext != undefined) {
					$scope.baseNext();
				}
			}
			
			$scope.qvDialogContext.nextDisabled = function() {
				return $scope.tables.filter((table) => table.checked || table.indeterminate).length == 0;
			}
			
			$scope.dataconnectionSelectionEditor.setMetadataModelForConnection({});

			function init() {
				$scope.inlineMode = true;
				$scope.qvDialogContext.size = "L";
				$scope.qvDialogContext.title = "Select data to load";

				$scope.cancelIsInProgress = false;

				$scope.databases = [];
				$scope.owners = [];
				$scope.tables = [];
				
				$scope.selectedTable = undefined;
				
				getDatabases().then((db) => $scope.databases = db);
				
			}
			
			$scope.onOK = function () {
				$scope.destroyComponent();
			};

			$scope.onCancel = function () {
				$scope.cancelIsInProgress = true;
			    $scope.destroyComponent();
			};

			$scope.onEscape = function () {
				$scope.destroyComponent();
			}
			
			$scope.onDatabaseInputChanged = function() {
				if ($scope.databaseInput && $scope.databaseInput.qName) {
					getOwners($scope.databaseInput.qName).then((owners) => $scope.owners = owners).then(() => console.log($scope.owners));
				} else {
					$scope.owners = [];
					$scope.tables = [];
					$scope.selectedTable = undefined;
				}
			}
			
			$scope.onOwnerInputChanged = function() {
				if ($scope.ownerInput) {
					getTables($scope.databaseInput.qName, $scope.ownerInput.qName).then((tables) => $scope.tables = tables);
				} else {
					$scope.tables = [];
					$scope.selectedTable = undefined;
				}
			}
			
			$scope.onTableShowClick = function(table) {
				return _onTableShow(table);
			}
			
			$scope.onTableCheckboxClick = function (table) {
				table.checked = !table.checked;
				table.indeterminate = false;

				return _onTableShow(table)
					.then(([fields, preview]) => {
						fields.forEach((field) => field.checked = table.checked)
					})
			};
			
			$scope.onFieldCheckboxClick = function (field) {
				field.checked = !field.checked;
				
				if (field.table.fields.every((field) => !field.checked)) {
					field.table.checked = false;
					field.table.indeterminate = false;
				} else if (field.table.fields.every((field) => field.checked)) {
					field.table.checked = true;
					field.table.indeterminate = false;
				} else {
					field.table.checked = false;
					field.table.indeterminate = true;
				}
			}
			
			$scope.isTableSelected = function (table) {
				return (table === $scope.selectedTable);
			};
			
			$scope.getNumberOfFields = function (table) {
				if (!table.fields || table.fields.length == 0) return undefined;
				return (table.fields.filter((field) => field.checked).length);
			};

			init();

		}]
	};
});