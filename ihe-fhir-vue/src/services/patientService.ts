import axios from 'axios'

const API_URL = "http://localhost:5000/api/patient";

export default {
    searchPatients(name: string) {
        return axios.get(`${API_URL}/search?name=${name}`)
    }
}