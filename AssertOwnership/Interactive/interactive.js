function main() {
    $.ajax({
        url: "https://fcg-arcgis-srv/portal/ownership/rest/group",
        dataType: "json",
        success: function (data) {
            let table = $("group-items-table")
            for (var key in data) {
                if (data.hasOwnProperty(key)) {
                    let row = table.insertRow();
                    row.insertCell().innerHTML = data[key]["title"];
                    row.insertCell().innerHTML = data[key]["type"];
                    row.insertCell().innerHTML = data[key]["owner"];

                    var groups = [];
                    for (var accessType in data[key]["groups"]) {
                        for (var group in data[key]["groups"][accessType]) {
                            groups.push(data[key]["groups"][accessType][group]["title"]);
                        }
                    }
                    var groupsText = groups.join(", ");
                    row.insertCell().innerHTML = groupsText;

                    // 
                    row.insertCell().innerHTML = data[key]["title"];
                }
            }
            
        }
    });
}