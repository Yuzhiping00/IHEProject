import {defineStore} from 'pinia'
import Patient from '@/models/Patient'
import patientService from '@/services/resources/patientService'

export const usePatientStore = defineStore("patient", {
    state: () => {
        return {
            patient: new Patient(),
        }
    },
    actions: {
        async setPatient() {
            const response = await patientService.query()
            if(response.status === 200 && response.data && response.data.length) {
                this.patient = response.data.find(Boolean) as Patient
                return true
            }
            return false
        }
    }
})