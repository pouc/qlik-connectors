define([
	'qvangular',
	'underscore',
	'text!QlikSimpleConnector.webroot/connect/connect-datamanager.html',
	'css!QlikSimpleConnector.webroot/connect/connect-datamanager.css',
],
function (qvangular, _, template, css) {
	return {
		template: template,
		scope: {
			serverside: '=',
			continueWithNewConnection: '=',
			qvDialogContext: '=',
			qvxSelectedEntrypoint: '='
		},

		controller: ['$scope', 'input', '$q', function ($scope, input, $q) {

            function init() {

				$scope.isLoading = true;
				
				$scope.isModify = input.editMode;
				$scope.qvDialogContext.size = "S";
				$scope.qvDialogContext.title = $scope.isModify ? "Change Qlik Simple Connector connection" : "Add Qlik Simple Connector connection";

                $scope.id = input.instanceId;
                $scope.connectionName = "";

                $scope.username = "";
                $scope.password = "";
                $scope.provider = "QlikSimpleConnector.exe";

                $scope.clearConnection();

                const getInfo = input.serverside.sendJsonRequest("getInfo").then(function (info) {
                    $scope.info = info.qMessage;
                });

                const getDrivers = input.serverside.sendJsonRequest("getDrivers").then(function (drivers) {
                    $scope.drivers = drivers.qDrivers;
                });

                if ($scope.isModify) {
                    input.serverside.getConnection($scope.id).then(function (result) {
                        $scope.connectionName = result.qName;
                    });
                }

				$q.all([getInfo, getDrivers]).then(() => {
					$scope.isLoading = false;
				})

            }
			
			$scope.saveConnection = function () {			
				if ($scope.isModify) {
					var overrideCredentials = ($scope.username !== "" && $scope.password !== "");

					return input.serverside.sendJsonRequest("storeParams",
						[{
							paramName: 'qConnectionId',
							paramType: 0,
							paramValueType: 1,
							selectedValues: [{ Key: $scope.id, Value: null }]
						}].concat([{
							paramName: 'qDriver',
							paramType: 0,
							paramValueType: 1,
							selectedValues: [{ Key: $scope.driverInput, Value: null }]
						}]).concat($scope.driverParams)
					);

				} else {

					return input.serverside.createNewConnection(
						$scope.connectionName,
						createCustomConnectionString($scope.provider, "FOO=BAR"),
						"FOO",
						"BAR"
					).then(function (result) {
						if (result) {
							return input.serverside.sendJsonRequest("createConnectionString",
								[{
									paramName: 'qConnectionId',
									paramType: 0,
									paramValueType: 1,
									selectedValues: [{ Key: result.qConnectionId, Value: null }]
								}]
							).then(function (info) {
								if (info.qOk) {

									return input.serverside.modifyConnection(
										result.qConnectionId,
										$scope.connectionName,
										createCustomConnectionString($scope.provider, info.qMessage),
										$scope.provider,
										true,
										"FOO",
										"BAR"
									).then(() => {

										return input.serverside.sendJsonRequest("storeParams",
											[{
												paramName: 'qConnectionId',
												paramType: 0,
												paramValueType: 1,
												selectedValues: [{ Key: result.qConnectionId, Value: null }]
											}].concat([{
												paramName: 'qDriver',
												paramType: 0,
												paramValueType: 1,
												selectedValues: [{ Key: $scope.driverInput, Value: null }]
											}]).concat($scope.driverParams)
										);
									
									});

								}
							});
						}
					});
				}
			}
			
			$scope.testConnection = function(showSucceedMessage) {
				return input.serverside.sendJsonRequest("testConnection",
					[{
					    paramName: 'qDriver',
					    paramType: 0,
					    paramValueType: 1,
					    selectedValues: [{ Key: $scope.driverInput, Value: null }]
					}].concat($scope.driverParams)
				).then(function (info) {
					if (showSucceedMessage) {
						$scope.connectionInfo = info.qMessage;
						$scope.connectionSuccessful = info.qMessage.indexOf("OK") !== -1;
					}
				})
			}
			
			$scope.onTestConnectionClicked = function () {

			};

			$scope.clearConnection = function () {
				$scope.connectionInfo = "";
				$scope.connectionSuccessful = false;
				$scope.driverInfo = "";
				$scope.driverParams = {};
				$scope.testConnect = "";
			}

			$scope.isOkEnabled = function () {
				return $scope.connectionSuccessful;
			};

			$scope.onEscape = $scope.onCancelClicked = function () {
				$scope.destroyComponent();
			};
			
			$scope.onDriverInputClicked = function () {
				$scope.clearConnection();
				
				if ($scope.driverInput != null) {
					$scope.isLoading = true;
					input.serverside.sendJsonRequest("testDriver", [{
						paramName: 'qDriver',
						paramType: 0,
						paramValueType: 1,
						selectedValues: [{ Key: $scope.driverInput, Value: null }]
					}]).then(function (info) {
						$scope.driverInfo = info.qMessage;

						if (info.qOk) {
							return input.serverside.sendJsonRequest("getDriverConnectParams", [{
								paramName: 'qDriver',
								paramType: 0,
								paramValueType: 1,
								selectedValues: [{ Key: $scope.driverInput, Value: null }]
							}]).then(function (params) {
								$scope.driverParams = params.qDriverParamList;
							});
						}
					}).then(() => {
						$scope.isLoading = false;
					});
				}
			};
			
			$scope.qvDialogContext.nextDisabled = nextDisabled;
			
			$scope.qvDialogContext.next = function () {
				$scope.isLoading = true;
				
				$scope.testConnection(false).then(() => {
					return $scope.saveConnection()
				}).then(() => {
					$scope.isLoading = false;
				}).then(() => {
					return input.continueWithNewConnection( {
						connectionName: $scope.connectionName
					} );
				})
			}

            /* Helper functions */
			
			function nextDisabled () {
				return $scope.isLoading || !$scope.connectionName;
			}

            function createCustomConnectionString(filename, connectionstring) {
                return "CUSTOM CONNECT TO " + "\"provider=" + filename + ";" + connectionstring + ";\"";
            }

            init();
			
		}]
	};
});
