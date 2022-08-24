$(() => {
    let selectedPost = null;

    const url = BASE_URL + "api/blogpost";
    var gridDataSource = new DevExpress.data.CustomStore({
        key: "id",
        load: async function () {

         return await fetch("api/session")
                .then((resp) => resp.json())
                .then(async function (data) {
                    return await fetch(url, {
                        method: 'GET',
                        headers: {
                            'Authorization': 'Bearer ' + data.token,
                        }
                    });
                })
                .then((resp) => resp.json())
                .then(async function (json) {
                    console.log(json.data);
                    return json.data;
                })
                .catch(function (error) {
                    console.log(error);
                    return [];
                });
        },
        insert: async function (values) {
            request = await fetch(`${url}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(values)
            });
            let response = await request.json();
            showToast(response.message, response.isSuccess);

        },
        update: async function (key, values) {
            values.id = key;

            request = await fetch(`${url}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(values)
            });
            let response = await request.json();
            showToast(response.message, response.isSuccess);
        },
        remove: function (key) {
            // ...
        }
    });

    const dataGrid = $('#gridContainer').dxDataGrid({
        dataSource: gridDataSource,
        remoteOperations: false,
        paging: {
            pageSize: 10,
        },
        pager: {
            showPageSizeSelector: true,
            allowedPageSizes: [10, 25, 50],
            showNavigationButtons: true
        },
        searchPanel: {
            visible: true,
            highlightCaseSensitive: false,
            location: 'before'
        },
        groupPanel: { visible: false },
        grouping: {
            autoExpandAll: false,
        },
        editing: {
            mode: "popup",
            allowUpdating: true,
            allowAdding: true,
            popup: {
                title: 'Post details',
                showTitle: true,
                width: 650,
                height: 600,
            },
            form: {
                items:
                    [
                        {
                            itemType: 'group',
                            colCount: 1,
                            colSpan: 2,
                            items: [
                                { dataField: "title" },                                
                            ],
                        },                        
                        {
                            itemType: 'group',
                            colCount: 1,
                            colSpan: 2,
                            items: [                                
                                { dataField: "content" },
                            ],
                        },
                    ],
            }
        },
        allowColumnReordering: true,
        rowAlternationEnabled: true,
        showBorders: true,
        columnsAutoWidth: true,
        columns: [
            {
                dataField: 'id',
                caption: "ID",
                //width: '30',
                formItem: {
                    visible: false
                }
            },
            {
                caption: "Author",
                dataField: 'userName',
                dataType: 'string',
                //validationRules: [{ type: "required" }]
            },
            {
                caption: "Title",
                dataField: 'title',
                width: '40%',
                dataType: 'string',
                validationRules: [{ type: "required" }]
            },
            {
                dataField: 'content',
                visible: false,
                formItem: {
                    visible: true
                },
                validationRules: [{ type: "required" }]
            },
            {
                caption: "Publish Date",
                dataField: 'publishDate',
                dataType: 'date',
            },
            {
                caption: "Comments",
                dataField: 'commentsCount'
            },
            {
                type: "buttons",
                width: 120,
                fixed: true,
                buttons: [
                    'edit',
                    {
                        hint: "Acceso app",
                        icon: "key",
                        //visible: function (e) {
                        //    return e.row.data.something === 0 ? false : true;
                        //},
                        onClick: function (e) {
                            selectedPost = e.row.data;

                            //popUpAppCredentials.option({ contentTemplate: () => popupAppCredentialsContentTemplate(e) });
                            //popUpAppCredentials.show();

                            e.event.preventDefault();
                        }
                    },
                    {
                        hint: "Sucursales",
                        icon: "bulletlist",
                        onClick: function (e) {
                            selectedPost = e.row.data;

                            //popupVendorBranchOffices.option("title", `Sucursales asignadas a: ${selectedVendor.name} ${selectedVendor.lastName}`);
                            //popupVendorBranchOffices.option({ contentTemplate: () => vendorBranchOfficeContentTemplate() });
                            //popupVendorBranchOffices.show();

                            //branchOfficesGrid.selectRows([]);
                            //branchOfficesGrid.selectRows(selectedVendor.branchOfficeIds);

                            e.event.preventDefault();
                        }
                    }
                ]
            }
        ],
        //toolbar: {
        //    items: [
        //        {
        //            location: 'after',
        //            widget: 'dxButton',
        //            options: {
        //                icon: 'edit',
        //                onClick() {
        //                    dataGrid.refresh();
        //                }
        //            }

        //        }
        //    ]
        //},
        onContentReady(e) {
        }
    }).dxDataGrid('instance');
});