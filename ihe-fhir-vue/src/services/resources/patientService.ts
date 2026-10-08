import { ApiService } from "@/services/core/apiService";
import Patient from "@/models/Patient";

// const API_URL = "http://localhost:5000/api/patient";

class PatientService extends ApiService<Patient> {
  resourcePath: string = "patient";

  //convert FHIR -> flat model
  private fromFhir(fhirPatient: any) {
    return new Patient({
      id: fhirPatient.id,
      familyName: fhirPatient.name?.[0]?.family ?? "",
      givenName: fhirPatient.name?.[0]?.given?.[0] ?? "",
      gender: fhirPatient.gender,
      birthDate: fhirPatient.birthDate,
    });
  }

  async queryPatients(): Promise<Patient[]> {
    const response = await this.query();

    if (response.status != 200) {
      throw new Error("Failed to load patients");
    }

    return (
      response.data.entry?.map((entry) => this.fromFhir(entry.resource)) ?? []
    );
  }

  async getMyPatient(): Promise<Patient> {
    const response = await this.axios.get(`${this.path}/me`);

    if (response.status !== 200) {
      throw new Error("Failed to load patient information");
    }

    return this.fromFhir(response.data);
  }

  async getPatientById(id: string): Promise<Patient> {
    const response = await this.get(id)

    if(response.status != 200)
    {
      throw new Error("Failed to load patient")
    }

    return this.fromFhir(response.data)
  }
}

export default new PatientService();
