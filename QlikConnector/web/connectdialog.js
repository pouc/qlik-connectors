if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}



define(['qvangular',
		'text!QlikConnector.webroot/connectdialog.ng.html',
		'css!QlikConnector.webroot/connectdialog.css'
], function (qvangular, template) {
    return {
        template: template,
        controller: ['$scope', 'input', function ($scope, input) {
            function init() {

                $scope.isEdit = input.editMode;
                $scope.id = input.instanceId;
                $scope.titleText = $scope.isEdit ? "Change Qlik Connector connection" : "Add Qlik Connector connection";
                $scope.saveButtonText = $scope.isEdit ? "Save changes" : "Create";

                $scope.name = "";

                $scope.username = "";
                $scope.password = "";
                $scope.provider = "QlikConnector.exe";

                $scope.clearConnection();

                input.serverside.sendJsonRequest("getInfo").then(function (info) {
                    $scope.info = info.qMessage;
                });

                input.serverside.sendJsonRequest("getDrivers").then(function (drivers) {
                    $scope.drivers = drivers.qDrivers;
                });

                if ($scope.isEdit) {
                    input.serverside.getConnection($scope.id).then(function (result) {
                        $scope.name = result.qName;
                    });
                }
            }

            $scope.onDriverInputClicked = function () {

                $scope.driverInfo = "";
                $scope.driverParams = {};
                $scope.clearConnection();

                $scope.testConnect = "";
                if ($scope.driverInput != null) {
                    input.serverside.sendJsonRequest("testDriver", [{
                        paramName: 'qDriver',
                        paramType: 0,
                        paramValueType: 1,
                        selectedValues: [{ Key: $scope.driverInput, Value: null }]
                    }]).then(function (info) {
                        $scope.driverInfo = info.qMessage;

                        if (info.qOk) {
                            input.serverside.sendJsonRequest("getDriverConnectParams", [{
                                paramName: 'qDriver',
                                paramType: 0,
                                paramValueType: 1,
                                selectedValues: [{ Key: $scope.driverInput, Value: null }]
                            }]).then(function (params) {
                                $scope.driverParams = params.qDriverParamList;
                            });
                        }
                    });
                }

            };


            /* Event handlers */

            $scope.onOKClicked = function () {
                if ($scope.isEdit) {
                    var overrideCredentials = ($scope.username !== "" && $scope.password !== "");

                    input.serverside.sendJsonRequest("storeParams",
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

                    input.serverside.createNewConnection(
						$scope.name,
						createCustomConnectionString($scope.provider, "FOO=BAR"),
						"FOO",
						"BAR"
					).then(function (result) {
					    if (result) {
					        input.serverside.sendJsonRequest("createConnectionString",
								[{
								    paramName: 'qConnectionId',
								    paramType: 0,
								    paramValueType: 1,
								    selectedValues: [{ Key: result.qConnectionId, Value: null }]
								}]
							).then(function (info) {
							    if (info.qOk) {

							        input.serverside.modifyConnection(
										result.qConnectionId,
										$scope.name,
										createCustomConnectionString($scope.provider, info.qMessage),
										$scope.provider,
										true,
										"FOO",
										"BAR"
									);

							        input.serverside.sendJsonRequest("storeParams",
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

							        $scope.destroyComponent();

							    }
							});
					    }
					});




                }
            };

            $scope.onTestConnectionClicked = function () {

                input.serverside.sendJsonRequest("testConnection",
					[{
					    paramName: 'qDriver',
					    paramType: 0,
					    paramValueType: 1,
					    selectedValues: [{ Key: $scope.driverInput, Value: null }]
					}].concat($scope.driverParams)
				).then(function (info) {
				    $scope.connectionInfo = info.qMessage;
				    $scope.connectionSuccessful = info.qMessage.indexOf("OK") !== -1;
				});

            };

            $scope.clearConnection = function () {
                $scope.connectionInfo = "";
                $scope.connectionSuccessful = false;
            }

            $scope.isOkEnabled = function () {
                return $scope.connectionSuccessful;
            };

            $scope.onEscape = $scope.onCancelClicked = function () {
                $scope.destroyComponent();
            };


            /* Helper functions */

            function createCustomConnectionString(filename, connectionstring) {
                return "CUSTOM CONNECT TO " + "\"provider=" + filename + ";" + connectionstring + "\"";
            }

            init();
        }]
    };
});