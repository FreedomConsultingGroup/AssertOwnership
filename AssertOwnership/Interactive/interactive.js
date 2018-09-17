var portalUrl = "https://fcg-arcgis-srv.freedom.local/portal/";

var user = "";
$.ajax({
    dataType: "json",
    url: portalUrl + "ownership/rest/whoami",
    success: function (data) {
        user = data["name"];
    }
});

function successFunction(data) {
    let table = $("#group-items-table")[0];
    for (var id in data) {
        if (data.hasOwnProperty(id)) {
            let row = table.insertRow(-1);

            let title = document.createElement("a");
            title.setAttribute("href", portalUrl + "home/item.html?id=" + id);
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
                a.setAttribute("href", portalUrl + "home/group.html?id=" + group["id"]);
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

            if (data[id]["owner"] != user) {
                var chownButton = document.createElement("button");
                chownButton.addEventListener("click", generateOnClickFunction(id));
                chownButton.appendChild(document.createTextNode("Assert Ownership"));
                row.insertCell(-1).appendChild(chownButton);
            } else {
                row.insertCell(-1);
            }
        }
    }
}

function generateOnClickFunction(id) {
    return function (evt) {
        $.ajax({
            method: "POST",
            dataType: "json",
            url: portalUrl + "ownership/rest/chown",
            data: {
                "id": id
            },
            success: function (data) {
                if (data["success"] == true) {
                    alert("Item successfully transfered")
                } else {
                    alert("Something went wrong: \n" + data["error"]["message"])
                }
            }
        });
    };
}

function main() {
    $.ajax({
        url: portalUrl + "ownership/rest/group",
        dataType: "json",
        success: successFunction
    });
}
main();