function backNavigate() {
    window.history.back();
}

function SaveTheme() {
    var title = document.getElementById("title").value;
    var content = document.getElementById("content").value;
    var image = document.getElementById("image").files[0];
    var token = localStorage.getItem("token");

    var formData = new FormData();

    formData.append("title", title);
    formData.append("content", content);
    formData.append("image", image);
    formData.append("token", token);

    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/Theme/SaveTheme", true); 
    xhr.onload = function () {
        if (xhr.status === 200) {
            // Uspešno poslat zahtev
            console.log("Podaci uspešno poslati.");
            window.location.href = '../User/UserPage';
        } else {
            // Greška prilikom slanja zahteva
            console.error("Greška prilikom slanja podataka.");
        }
    };
    xhr.onerror = function () {
        // Greška u konekciji
        console.error("Greška u konekciji.");
    };
    xhr.send(formData); // Slanje podataka FormData objektom

    // Vraćanje false kako bi se sprečilo podnošenje običnog zahteva forme
    return false;
}
