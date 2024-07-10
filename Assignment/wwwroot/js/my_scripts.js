let currentPage = 1;
let currentSearchTerm = '';

function handleSearch() {
    currentSearchTerm = document.getElementById('searchTerm').value;
    currentPage = 1;
    document.getElementById('photoGallery').innerHTML = ''; // Clear previous results
    fetchPhotos(currentSearchTerm, currentPage);
}

function loadMorePhotos() {
    currentPage++;
    fetchPhotos(currentSearchTerm, currentPage);
}

function fetchPhotos(searchTerm, page) {
    const loading = document.getElementById('loading');
    const error = document.getElementById('error');
    const loadMoreContainer = document.getElementById('loadMoreContainer');

    loading.style.display = 'block';
    error.style.display = 'none';
    loadMoreContainer.style.display = 'none'; // Hide the load more button while loading

    console.log(`Fetching photos for term: ${searchTerm}, page: ${page}`);

    fetch(`/api/photos/search?searchTerm=${searchTerm}&page=${page}`)
        .then(response => response.json())
        .then(data => {
            loading.style.display = 'none';
            renderPhotos(data);

            if (data.length > 0) {
                console.log('Photos fetched successfully, showing load more button.');
                loadMoreContainer.style.display = 'block'; // Show the load more button if there are more photos
            } else {
                console.log('No more photos found.');
                loadMoreContainer.style.display = 'none'; // Hide the load more button if no more photos are found
            }
        })
        .catch(error => {
            console.error('Error fetching photos:', error);
            loading.style.display = 'none';
            error.style.display = 'block';
            loadMoreContainer.style.display = 'block'; // Show the load more button if an error occurs
        });
}

function renderPhotos(photos) {
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
        // photoDiv.appendChild(title);
        photoGallery.appendChild(photoDiv);
    });
}




