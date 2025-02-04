import axios from 'axios'

const API_URL = "http://localhost:5000/api/pdq";

export default {
    searchPatients(name) {
        return axios.get(`${API_URL}/search?name=${name}`)
    }
}