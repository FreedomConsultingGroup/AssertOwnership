var portalUrl = "https://fcg-arcgis-srv.freedom.local/portal/";

var user = "";
var thumbnail = "";
ajax(portalUrl + "ownership/rest/whoami", "GET", null, function (data) {
    user = data["name"];
    document.getElementById("username").innerHTML = user;

    let thumbnail = document.getElementById("avatar");
    if (data["thumbnail"] === null) {
        thumbnail.src = portalUrl + "/home/10.6/js/arcgisonline/css/images/no-user-thumb.jpg";
    } else {
        thumbnail.src = portalUrl + "/sharing/rest/community/users/" + user + "/info/" + data["thumbnail"];
    }
});

function successFunction(data) {
    let table = document.getElementById("group-items-table");
    for (var id in data) {
        if (data.hasOwnProperty(id)) {
            let row = table.insertRow(-1);

            let title = document.createElement("a");
            title.setAttribute("href", portalUrl + "home/item.html?id=" + id);
            title.setAttribute("target", "_blank");
            title.setAttribute("rel", "noopener noreferrer");
            title.appendChild(document.createTextNode(data[id]["title"]));
            row.insertCell(-1).appendChild(title);

            row.insertCell(-1).appendChild(document.createTextNode(data[id]["type"]));

            row.insertCell(-1).appendChild(document.createTextNode(data[id]["owner"]));

            let groups = [];
            for (var i in data[id]["groups"]) {
                let group = data[id]["groups"][i];
                var a = document.createElement("a");
                a.setAttribute("href", portalUrl + "home/group.html?id=" + group["id"]);
                a.setAttribute("target", "_blank");
                a.setAttribute("rel", "noopener noreferrer");
                a.appendChild(document.createTextNode(group["title"]));
                groups.push(a);
            }
            let cell = row.insertCell(-1);
            for (var group in groups) {
                cell.appendChild(groups[group]);
                if (group !== groups.length - 1) {
                    cell.appendChild(document.createTextNode(", "));
                }
            }

            let tags = [];
            for (var tag in data[id]["tags"]) {
                tags.push(data[id]["tags"][tag]);
            }
            let tagsText = tags.join(", ");
            row.insertCell(-1).appendChild(document.createTextNode(data[id]["tags"]));

            if (data[id]["owner"] !== user) {
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
        ajax(portalUrl + "ownership/rest/chown", "POST", { "id": id }, function (data) {
            if (data["success"] === true) {
                alert("Item successfully transfered");
            } else {
                alert("Something went wrong: \n" + data["error"]["message"]);
            }
        });
    };
}

function main() {
    ajax(portalUrl + "ownership/rest/group", "GET", null, successFunction);
}

function ajax(url, method, data, success) {
    var reqObj = new XMLHttpRequest();
    reqObj.responseType = "json";
    reqObj.onreadystatechange = function () {
        if (this.readyState === 4 && this.status === 200) {
            success(this.response);
        }
    };
    if (method === "POST") {
        reqObj.open(method, url);
        reqObj.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
        reqObj.send(data);
    } else if (method === "GET") {
        if (data !== null) {
            let reqData = [];
            for (let key in data) {
                reqData.push(encodeURIComponent(key) + "=" + encodeURIComponent(data[key]));
            }
            reqObj.open(method, url + "?" + reqData.join("&"));
            reqObj.send();
        } else {
            reqObj.open(method, url);
            reqObj.send();
        }
    }
}
main();