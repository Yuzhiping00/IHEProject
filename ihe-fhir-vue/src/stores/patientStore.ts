import { defineStore } from "pinia";
import Patient from "@/models/Patient";
import patientService from "@/services/resources/patientService";

export const usePatientStore = defineStore("patient", {
  state: () => {
    return {
      patient: new Patient(),
    };
  },
  actions: {
    async setPatient() {
      const response = await patientService.query();
      if (response.status === 200 && response.data && response.data.length) {
        this.patient = response.data.find(Boolean) as Patient;
        return true;
      }
      return false;
    },

    //convert flat model to FHIR

    toFhir(): any {
      return {
        resourceType: "Patient",
        id: this.patient.id,
        gender: this.patient.gender,
        birthDate: this.patient.birthDate,
        name: [
          {
            family: this.patient.familyName,
            given: [this.patient.givenName],
          },
        ],
      };
    },

    //convert FHIR -> flat model
    fromFhir(fhirPatient: any) {
      this.patient = new Patient({
        id: fhirPatient.id,
        familyName: fhirPatient.name?.[0].family || "",
        givenName: fhirPatient.name?.[0]?.given?.[0] || "",
        gender: fhirPatient.gender,
        birthDate: fhirPatient.birthDate,
      });
    },

    // reset to blank patient
    clearPatient() {
      this.patient = new Patient();
    }

    // convertDateToString(){
    //   //Format birthdate if present
    //   if (this.patient.birthDate) {
    //     // parse date string into a date object
    //     const date = new Date(this.patient.birthDate);

    //     //convert to YYYY-MM-DD
    //     this.patient.birthDate = date.toISOString().split("T")[0];
    //   }
    // },
  },
});
