import { defineStore } from "pinia";
import Patient from "@/models/Patient";
import patientService from "@/services/resources/patientService";
import { computed } from 'vue'

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
    },

   // 创建一个通用 computed 双向绑定方法
    createDateProxy(fieldName: keyof Patient) {
      return computed({
        get: () => {
          const value = this.patient[fieldName]
          if (!value) return null
          const [year, month, day] = value.split('-').map(Number)
          return new Date(year, month - 1, day)
        },
        set: (val: Date | null) => {
          if (!val) {
            this.patient[fieldName] = ''
            return
          }
          const year = val.getFullYear()
          const month = String(val.getMonth() + 1).padStart(2, '0')
          const day = String(val.getDate()).padStart(2, '0')
          this.patient[fieldName] = `${year}-${month}-${day}`
          debugger
          console.log("date of birth: ", this.patient[fieldName])
        }
      })
    }
  },
});
