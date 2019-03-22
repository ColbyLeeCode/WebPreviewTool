
//get all the images
var images = document.getElementsByTagName("img")
var imageCount = 16;
var current = 0;
var zLayerCount = 0;

//set all images visible to false
for (var i = 0; i < images.length; i++) {
    images[i].style.visibility = "hidden";
}



function previous() {     
        zLayerCount--;
        if(current > images.length - 1)
        {   
            zLayerCount--;  
            images[current%images.length].style.zIndex = zLayerCount;            
        }
        current--;
        
        update();
}

function next() {
        
        if(current > images.length - 1)
        {
            
            images[current%images.length].style.zIndex = current;
        }
        current++;
        update();
}

function update() {
    console.log("current: " + current);
    console.log(images[current%images.length].style.zIndex);
    for (var i = 0; i < images.length; i++) {
        if (i < current)
            images[i].style.visibility = "visible";
        else
        images[i].style.visibility = "hidden";
    }
}

