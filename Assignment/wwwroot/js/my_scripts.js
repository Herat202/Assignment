
document.addEventListener('DOMContentLoaded', (event) => {
    window.currentPage = 1;
    window.currentSearchTerm = '';
    window.isLoading = false;

    window.handleSearch = function() {
        window.currentSearchTerm = document.getElementById('searchTerm').value;
        window.currentPage = 1;
        document.getElementById('photoGallery').innerHTML = ''; // Clear previous results
        fetchPhotos(window.currentSearchTerm, window.currentPage);
    }

    window.fetchPhotos = function(searchTerm, page) {
        if (window.isLoading) return; // Prevent multiple simultaneous requests
        window.isLoading = true;

        const loading = document.getElementById('loading');
        const error = document.getElementById('error');
        loading.style.display = 'block';
        error.style.display = 'none';

        fetch(`/api/photos/search?searchTerm=${searchTerm}&page=${page}`)
            .then(response => response.json())
            .then(data => {
                loading.style.display = 'none';
                renderPhotos(data);
                window.isLoading = false; // Reset loading state
            })
            .catch(() => {
                loading.style.display = 'none';
                error.style.display = 'block';
                window.isLoading = false; // Reset loading state
            });
    }

    window.renderPhotos = function(photos) {
        const photoGallery = document.getElementById('photoGallery');
        photos.forEach(photo => {
            const photoDiv = document.createElement('div');
            photoDiv.className = 'photo';
            const img = document.createElement('img');
            img.src = photo.imageUrl;
            img.alt = photo.title;
            const title = document.createElement('p');
            title.textContent = photo.title;
            photoDiv.appendChild(img);
            photoDiv.appendChild(title);
            photoGallery.appendChild(photoDiv);
        });
    }

    // Endless scroll implementation
    const photoGalleryContainer = document.querySelector('.photo-gallery-container');
    photoGalleryContainer.addEventListener('scroll', () => {
        if ((photoGalleryContainer.scrollTop + photoGalleryContainer.clientHeight) >= photoGalleryContainer.scrollHeight - 100 && !window.isLoading) {
            window.currentPage++;
            fetchPhotos(window.currentSearchTerm, window.currentPage);
        }
    });
});

