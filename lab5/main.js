var url = "https://localhost:7200/api/images"

let inp = document.getElementById('input')
inp.addEventListener('change', upload_data)
let loc = location.pathname
load_data();

function select_files(){
    const file = document.getElementById('input')
    file.click()
}

async function load_data(){
    let response = await fetch(url);
    let ids = await response.json();
    for (let i = 0; i < ids.length; i++) {
        await add(ids[i]);
    }
}

async function add(id){
    let responce = await fetch(url + "/{0}".replace('{0}', id));
    let img_ = await responce.json();
    let wrapper = document.createElement('div');
    wrapper.setAttribute('class', 'wrapper');
    let item = document.createElement('img');
    let des = document.createElement('p');
    des.textContent = img_.descr;
    item.setAttribute('src', img_.path);
    wrapper.appendChild(item);
    wrapper.appendChild(des);
    switch(img_.category) {
        case 'neutral':
            document.getElementById('neutral').appendChild(wrapper);
            break;
        case 'happiness':
            document.getElementById('happiness').appendChild(wrapper);
            break;
        case 'surprise':
            document.getElementById('surprise').appendChild(wrapper);
            break;
        case 'sadness':
            document.getElementById('sadness').appendChild(wrapper);
            break;
        case 'anger':
            document.getElementById('anger').appendChild(wrapper);
            break;
        case 'disgust':
            document.getElementById('disgust').appendChild(wrapper);
            break;
        case 'fear':
            document.getElementById('fear').appendChild(wrapper);
            break;
        case 'contempt':
            document.getElementById('contempt').appendChild(wrapper);
            break;
        default:
            break;
    }
}

async function upload_data(event){
    let files = document.getElementById('input').files
    for (let f of files) {
        let name = f.name
        let prms = new Promise(res => {
            let reader = new FileReader()
            reader.onloadend = e => res(e.target.result)
            reader.readAsDataURL(f)
        })
        let img = (await prms).split(',')[1]
        const post_request = {
            mode: 'cors',
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                "img": img,
                "path": loc.slice(3, loc.length - 11) + "images/" + name
            })
        }
        let response = await fetch(url, post_request)
        let id = await response.json()
        if (id >= 0)
            await add(id)
        else
            alert("Это изображение уже есть в базе")
    }
}
