define(['qvangular'
], function (qvangular) {
    return ['serverside', 'standardSelectDialogService', 'dataconnectorInstance', function (serverside, standardSelectDialogService, dataconnectorInstance) {

        var eventlogDialogContentProvider = {
            getConnectionInfo: function () {
                return qvangular.promise({
                    dbusage: true,
                    ownerusage: false,
                    dbseparator: '.',
                    ownerseparator: '.',
                    specialchars: '! "$&\'()*+,-/:;<>`{}~[]',
                    quotesuffix: ']',
                    quoteprefix: '[',
                    dbfirst: true,
                    keywords: []
                });
            },

            getDatabases: function () {
                return serverside.sendJsonRequest("getDatabases", [{
                    paramName: 'qConnectionId',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: dataconnectorInstance.id, Value: null }]
                }]).then(function (response) {
                    return response.qDatabases;
                });
            },

            getOwners: function ( /*databaseName*/) {
                return qvangular.promise([{ qName: "" }]);
            },

            getTables: function (qDatabaseName, qOwnerName) {
                return serverside.sendJsonRequest("getTables", [{
                    paramName: 'qConnectionId',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: dataconnectorInstance.id, Value: null }]
                }].concat([{
                    paramName: 'qDatabase',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qDatabaseName, Value: null }]
                }])).then(function (response) {
                    return response.qTables;
                });

            },

            getFields: function (qDatabaseName, qOwnerName, qTableName) {
                return serverside.sendJsonRequest("getFields", [{
                    paramName: 'qConnectionId',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: dataconnectorInstance.id, Value: null }]
                }].concat([{
                    paramName: 'qDatabase',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qDatabaseName, Value: null }]
                }]).concat([{
                    paramName: 'qTable',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qTableName, Value: null }]
                }])).then(function (response) {
                    return response.qFields;
                });
            },

            getPreview: function (qDatabaseName, qOwnerName, qTableName) {
                return serverside.sendJsonRequest("getPreview", [{
                    paramName: 'qConnectionId',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: dataconnectorInstance.id, Value: null }]
                }].concat([{
                    paramName: 'qDatabase',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qDatabaseName, Value: null }]
                }]).concat([{
                    paramName: 'qTable',
                    paramType: 0,
                    paramValueType: 1,
                    selectedValues: [{ Key: qTableName, Value: null }]
                }])).then(function (response) {
                    return response.qPreview;
                });
            }
        };

        console.log(dataconnectorInstance);

        standardSelectDialogService.showStandardDialog(eventlogDialogContentProvider, {
            precedingLoadVisible: true,
            fieldsAreSelectable: true,
            allowFieldRename: true
        });
    }];
});



