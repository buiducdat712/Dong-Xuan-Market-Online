$(function () {

    if ($("a.confirmDeletion").length) {
        $("a.confirmDeletion").click(() => {
            if (!confirm("Confirm deletion")) return false;
        });
    }

    if ($("div.alert.notification").length) {
        setTimeout(() => {
            $("div.alert.notification").fadeOut();
        }, 2000);
    }

});

function readURL(input) {
    if (input.files && input.files[0]) {
        let reader = new FileReader();

        reader.onload = function (e) {
            $("img#imgpreview").attr("src", e.target.result).width(100).height(100);
        };

        reader.readAsDataURL(input.files[0]);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const customerReviewLink = document.getElementById('customer-review-link');
    const reviewSection = document.getElementById('review-section');

    customerReviewLink.addEventListener('click', (event) => {
        event.preventDefault();

        // Scroll into view first
        reviewSection.scrollIntoView({ behavior: 'smooth' });

        // Adjust the scroll position after a short delay to ensure it has scrolled into view
        setTimeout(() => {
            const offset = 100; // Adjust this value to set how much higher you want the section to be
            window.scrollBy({
                top: -offset,
                behavior: 'smooth'
            });
        }, 500); // Delay to ensure the scrollIntoView has completed
    });
});
