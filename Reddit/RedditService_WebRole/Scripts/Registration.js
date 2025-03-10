function validateForm() {
    var ime = document.getElementById("Ime").value.trim();
    var prezime = document.getElementById("Prezime").value.trim();
    var adresa = document.getElementById("Adresa").value.trim();
    var grad = document.getElementById("Grad").value.trim();
    var drzava = document.getElementById("Drzava").value.trim();
    var brojTelefona = document.getElementById("Broj_telefona").value.trim();
    var email = document.getElementById("Email").value.trim();
    var password = document.getElementById("Password").value.trim();
    var profilePicture = document.getElementById("profilePicture").value.trim();

    if (ime === "") {
        alert("Molimo unesite ime.");
        return false;
    }

    if (prezime === "") {
        alert("Molimo unesite prezime.");
        return false;
    }

    if (adresa === "") {
        alert("Molimo unesite adresu.");
        return false;
    }

    if (grad === "") {
        alert("Molimo unesite grad.");
        return false;
    }

    if (drzava === "") {
        alert("Molimo unesite državu.");
        return false;
    }

    if (brojTelefona === "") {
        alert("Molimo unesite broj telefona.");
        return false;
    } else if (!/^\d+$/.test(brojTelefona)) {
        alert("Broj telefona može sadržavati samo cifre.");
        return false;
    }

    if (email === "") {
        alert("Molimo unesite email adresu.");
        return false;
    } else if (!isValidEmail(email)) {
        alert("Molimo unesite validnu email adresu.");
        return false;
    }

    if (password === "") {
        alert("Molimo unesite lozinku.");
        return false;
    } else if (password.length < 8) {
        alert("Lozinka mora sadržavati najmanje 8 karaktera.");
        return false;
    }

    if (profilePicture === "") {
        alert("Molimo izaberite profilnu sliku.");
        return false;
    }

    return true;
}

function isValidEmail(email) {
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}
