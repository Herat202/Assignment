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
        
        // When loading starts, display the loading spinner and hide the error message
        loading.style.display = 'block';
        loading.classList.add('active');
        error.style.display = 'none';

        // Set a timeout for the fetch request
        const controller = new AbortController();
        const timeoutId = setTimeout(() => {
            controller.abort(); // Abort the fetch request
            // When loading finishes or fails
            loading.classList.remove('active');
            loading.style.display = 'none';
            error.classList.add('active'); // On error
            error.textContent = 'Request timed out. Please try again later.';
            error.style.display = 'block';
            window.isLoading = false; // Reset loading state
        }, 10000); // Timeout after 10 seconds

        fetch(`/api/photos/search?searchTerm=${searchTerm}&page=${page}`)
            .then(response => {
                clearTimeout(timeoutId); // Clear the timeout if request is successful
                if (!response.ok) 
                {
                    console.log("response:", response);
                    // Extract the error message from the response body
                    return response.text().then(text => 
                        {
                            throw new Error(text);
                        });
                }
                return response.json();
            })
            .then(data => {
                loading.style.display = 'none';
                renderPhotos(data);
                window.isLoading = false; // Reset loading state
            })
            .catch((err) => {
                console.log(err.message);
                let partsByStart = err.message.split("Start");
                let partsByEnd = partsByStart[1].split("End");
                let finalMessage = partsByEnd[0];
                loading.style.display = 'none';
                error.innerHTML = `<p><span style="color: red;">Failed to load photos:</span><br><span style="color: black;">${finalMessage}</span></p>`;
                error.style.display = 'block';
                window.isLoading = false; // Reset loading state
            });
    }

    window.renderPhotos = function(photos) {
        const photoGallery = document.getElementById('photoGallery');
        console.log(photos);
        photos.forEach(photo => {
            console.log(photo);
            const photoDiv = document.createElement('div');
            photoDiv.className = 'photo';
            const img = document.createElement('img');
            img.src = photo.imageUrl;
            img.alt = photo.title;
            const title = document.createElement('p');
            title.textContent = photo.title;
            photoDiv.appendChild(img);
            photoDiv.appendChild(title);  // Perhaps nicer without title !?
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

