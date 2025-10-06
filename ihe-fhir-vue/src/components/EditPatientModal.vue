<script setup lang="ts">
import { usePatientStore } from '@/stores/patientStore';
import { ref, nextTick, computed} from 'vue';
import { VDateInput } from 'vuetify/lib/labs/components.mjs'

const patientStore = usePatientStore()
const maxDate = ref(new Date())
const minDate = "1900-01-01"
const items = ref(['Male', 'Female', 'Unknown', 'Other'])
const form = ref()

const props = defineProps({
    showModal: Boolean,
})

const emit = defineEmits(["update-patient", "cancel-update"])
const loading = ref(false)

//computed getter/setter to convert FHIR string <-> Date object for Vuetify
const birthDateProxy = computed({
  get: () => {
    const value = patientStore.patient.birthDate
    if (!value) return null
    // Convert "YYYY-MM-DD" string → Date safely
    const [year, month, day] = value.split('-').map(Number)
    return new Date(year, month - 1, day)
  },
  set: (val: Date | null) => {
    if (!val) {
      patientStore.patient.birthDate = ''
      return
    }
    // Convert Date → "YYYY-MM-DD" string
    patientStore.patient.birthDate = val.toISOString().split('T')[0]
  },
})

const firstNameRules = [
    (value: any) => value ? true : 'You must enter a patient first name',
    (value: any) => value?.length <= 20 ? true : "First name must be less than 20 characters",
    (value: any) => (/[^0-9]/.test(value)) ? true : "First name can not contain all digits"
]

const lastNameRules = [
    (value: any) => value ? true : 'You must enter a patient last name',
    (value: any) => value?.length <= 20 ? true : "Last name must be less than 20 characters",
    (value: any) => (/[^0-9]/.test(value)) ? true : "Last name can not contain all digits"
]

const savePatient = async () => {
    if(!form.value) {
        await nextTick()
    }

    if(!form.value) {
        return
    }
    
    loading.value = true
    const { valid } = await form.value.validate()
    loading.value = false
    if (!valid) return
    emit("update-patient")  
}

const cancelUpdate = () => {
    emit("cancel-update")
}
</script>

<template>
    <!-- Edit Patient Modal -->
    <v-dialog v-model="props.showModal" width="45%">
        <v-card>
            <v-card-title class="d-flex justify-space-between align-center">
                <div class="text-h5 ps-2">
                    Update Patient
                </div>
                <v-btn icon="mdi-close" variant="text" @click="cancelUpdate"></v-btn>
            </v-card-title>
            <v-card-text>
                <v-form ref="form">
                    <v-text-field label="Last Name" v-model="patientStore.patient.familyName" :rules="lastNameRules"
                        required />
                    <v-text-field label="First Name" v-model="patientStore.patient.givenName" :rules="firstNameRules"
                        required />
                    <v-select label="Gender" v-model="patientStore.patient.gender" :items="items"
                        :rules="[v => !!v || 'Patient Gender is required']" required />
                    <!-- Use computed date model for correct type handling -->
                    <v-date-input  v-model="birthDateProxy" clearable label="Birth of Date" :rules="[v => !!v || 'Patient Birth of Date is required']"
                        prepend-icon="" append-inner-icon="$calendar" :max="maxDate" :min="minDate"></v-date-input>

                    <v-container class="mt-6">
                        <v-row no-gutters justify="start">
                            <v-col cols="12" md="3">
                                <v-btn color="success" rounded="xl" class="mb-3" type="submit" @click="savePatient">
                                    Update
                                </v-btn>
                            </v-col>
                            <v-col cols="12" md="4">
                                <v-btn color="error" rounded="xl" class="mb-3" @click="cancelUpdate">
                                    Cancel
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-container>
                </v-form>
            </v-card-text>
        </v-card>
    </v-dialog>
</template>

<style scoped></style>
