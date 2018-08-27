var portalUrl = "https://fcg-arcgis-srv.freedom.local/portal"
function successFunction(data) {
    let table = $("#group-items-table")[0];
    for (var id in data) {
        if (data.hasOwnProperty(id)) {
            let row = table.insertRow(-1);

            let title = document.createElement("a");
            title.setAttribute("href", portalUrl + "/home/item.html?id=" + id);
            title.setAttribute("target", "_blank");
            title.setAttribute("rel", "noopener noreferrer")
            title.appendChild(document.createTextNode(data[id]["title"]));
            row.insertCell(-1).appendChild(title);

            row.insertCell(-1).appendChild(document.createTextNode(data[id]["type"]));

            row.insertCell(-1).appendChild(document.createTextNode(data[id]["owner"]));

            let groups = [];
            for (var i in data[id]["groups"]) {
                var group = data[id]["groups"][i];
                var a = document.createElement("a");
                a.setAttribute("href", portalUrl + "/home/group.html?id=" + group["id"]);
                a.setAttribute("target", "_blank");
                a.setAttribute("rel", "noopener noreferrer")
                a.appendChild(document.createTextNode(group["title"]));
                groups.push(a);
            }
            let cell = row.insertCell(-1);
            for (var group in groups) {
                cell.appendChild(groups[group]);
                if (group != groups.length - 1) {
                    cell.appendChild(document.createTextNode(", "));
                }
            }

            let tags = [];
            for (var tag in data[id]["tags"]) {
                tags.push(data[id]["tags"][tag]);
            }
            let tagsText = tags.join(", ");
            row.insertCell(-1).appendChild(document.createTextNode(data[id]["tags"]));
        }
    }
    document.styleSheets[0].
}

function main() {
    $.ajax({
        url: portalUrl + "/ownership/rest/group",
        dataType: "json",
        success: successFunction
    });
}
main();