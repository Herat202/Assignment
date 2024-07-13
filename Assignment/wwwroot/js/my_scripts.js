document.addEventListener('DOMContentLoaded', (event) => {
    window.currentPage = 1;
    window.currentSearchTerm = '';
    window.isLoading = false;
    window.currentFilter = "Relevant";

    // ************************************************************************************************
    // Handling/activating search if the following occurs: 
    // Enter key press on the search input (see above)
    // User presses the search button
    window.handleSearch = function() 
    {
        window.currentSearchTerm = document.getElementById('searchTerm').value;
        window.currentPage = 1;
        const filter = document.getElementById("filter").value;
        document.getElementById('photoGallery').innerHTML = ''; // Clear previous results
        if (window.currentSearchTerm) 
        {
            fetchPhotos(window.currentSearchTerm, window.currentPage, filter);
        } 
        else 
        {
            fetchPhotos("", window.currentPage, filter);
        }
    }

    // Add event listener for Enter key press on the search input
    const searchInput = document.getElementById('searchTerm');
    searchInput.addEventListener('keydown', (event) => 
    {
        if (event.key === 'Enter') 
        {
            event.preventDefault(); // Prevent form submission if inside a form
            window.handleSearch();
        }
    });

    // ************************************************************************************************
    // Search category: Relevance, Data uploaded, Date taken, Interesting
    window.handleFilterChange = function () 
    {
        window.currentSearchTerm = document.getElementById('searchTerm').value;
        window.currentPage = 1;
        const filter = document.getElementById("filter").value;
        document.getElementById('photoGallery').innerHTML = ''; // Clear previous results
        if (window.currentSearchTerm) 
        {
            fetchPhotos(window.currentSearchTerm, window.currentPage, filter);
        } 
        else 
        {
            fetchPhotos("", window.currentPage, filter);
        }
        window.currentFilter = filter;
    }

    // ************************************************************************************************
    // Fetching photos by sending API request to the applications REST API
    window.fetchPhotos = function(searchTerm, page, filter) 
    {
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
        const timeoutId = setTimeout(() => 
        {
            controller.abort(); // Abort the fetch request
            // When loading finishes or fails
            loading.classList.remove('active');
            loading.style.display = 'none';
            error.classList.add('active'); // On error
            error.textContent = 'Request timed out. Please try again later.';
            error.style.display = 'block';
            window.isLoading = false; // Reset loading state
        }, 10000); // Timeout after 10 seconds

        let url = "";
        if (searchTerm) 
        {
            // Search photos through Flickr search API
            // url = `/api/photos/search?searchTerm=${searchTerm}&page=${page}&sort=${filter}`;
            url = `/api/photos/search?searchTerm=${encodeURIComponent(searchTerm)}&page=${encodeURIComponent(page)}&sort=${encodeURIComponent(filter)}`;
        } 
        else 
        {
            // Get recent photos through Flickr GetRecent API
            document.getElementById('filter').value = "Date uploaded";
            url = `api/photos/GetRecent?&page=${page}`;
        }

        fetch(url)
            .then(response => 
            {
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
            .then(data => 
            {
                loading.style.display = 'none';
                renderPhotos(data);
                window.isLoading = false; // Reset loading state
            })
            .catch((err) => 
            {
                console.log(err.message);
                let partsByStart = err.message.split("Start");
                let partsByEnd = partsByStart[1].split("End");
                let finalMessage = partsByEnd[0];
                console.log("Final message:", finalMessage);
                loading.style.display = 'none';
                error.innerHTML = `<p><span style="color: red;">${finalMessage}</span><br><span style="color: black;"></span></p>`;
                error.style.display = 'block';
                window.isLoading = false; // Reset loading state
            }
        );
    }

    // ************************************************************************************************
    // Displaying photos on the page
    window.renderPhotos = function(photos) 
    {
        const photoGallery = document.getElementById('photoGallery');
        console.log(photos);
        photos.forEach(photo => 
        {
            console.log(photo);
            const photoDiv = document.createElement('div');
            photoDiv.className = 'photo';
            const img = document.createElement('img');
            img.src = photo.imageUrl;
            img.alt = photo.title;
            const title = document.createElement('p');
            title.textContent = photo.title;
            photoDiv.appendChild(img);
            // photoDiv.appendChild(title);  // Perhaps nicer without title !?
            photoGallery.appendChild(photoDiv);
        });
    }

    // ************************************************************************************************
    // Endless scroll implementation
    const photoGalleryContainer = document.querySelector('.photo-gallery-container');
    photoGalleryContainer.addEventListener('scroll', () => 
    {
        if ((photoGalleryContainer.scrollTop + photoGalleryContainer.clientHeight) >= photoGalleryContainer.scrollHeight - 100 && !window.isLoading) 
        {
            window.currentPage++;
            window.currentSearchTerm = document.getElementById('searchTerm').value;
            fetchPhotos(window.currentSearchTerm, window.currentPage, window.currentFilter);
        }
    });
});

