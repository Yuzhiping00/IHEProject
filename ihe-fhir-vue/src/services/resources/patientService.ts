import {ApiService} from "@/services/core/apiService"
import Patient from "@/models/Patient";

// const API_URL = "http://localhost:5000/api/patient";

class PatientService extends ApiService<Patient> {
    resourcePath: string ="patient";
}

export default new PatientService();