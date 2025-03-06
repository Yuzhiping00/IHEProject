<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import { usePatientStore } from '@/stores/patientStore'
import { VDateInput } from 'vuetify/lib/labs/components.mjs'

const router = useRouter()
const patientStore = usePatientStore()
const items = ref(['Male', 'Female', 'Unknown', 'Other'])
const patientForm = ref()
const isSubmitting = ref(false)
const showSuccessDialog = ref(false)
const showErrorSnackbar = ref(false)
const errorMessage = ref("")
const maxDate = ref(new Date())
const minDate = "1900-01-01"
const firstNameRules = [
    (value: any) => value ? true : 'You must enter a patinet first name',
    (value: any) => value?.length <= 20 ? true : "First name must be less than 20 characters",
    (value: any) => (/[^0-9]/.test(value)) ? true : "First name can not contain all digits"
]

const lastNameRules = [
    (value: any) => value ? true : 'You must enter a user last name',
    (value: any) => value?.length <= 20 ? true : "Last name must be less than 20 characters",
    (value: any) => (/[^0-9]/.test(value)) ? true : "Last name can not contain all digits"
]

const submitForm = async () => {
    const { valid } = await patientForm.value.validate()
    if (!valid) return

    isSubmitting.value = true;

    const response = await patientService.post(patientStore.patient)
    if (response && response.status === 200) {
        showSuccessDialog.value = true
        resetForm()
        resetValidation()
        router.push('/patients')
    } else {
        errorMessage.value = response?.statusText || "Failed to create patient."
        showErrorSnackbar.value = true
    }

    isSubmitting.value = false
}

const resetForm = () => patientForm.value.reset()
const resetValidation = () => patientForm.value.resetValidation()

const closeSuccessDialog = () => {
    showSuccessDialog.value = false
}

</script>

<template>
    <v-container width="50%">
        <v-card>
            <v-card-title class="my-6 text-uppercase">
                Create Patient
            </v-card-title>
            <v-card-text>
                <v-form @submit.prevent="submitForm" ref="patientForm">
                    <v-text-field v-model="patientStore.patient.familyName" label="Last Name" :counter="20"
                        :rules="firstNameRules" required />
                    <v-text-field v-model="patientStore.patient.givenName" label="First Name" :counter="20"
                        :rules="lastNameRules" required />
                    <v-select v-model="patientStore.patient.gender" label="Gender" :items="items"
                        :rules="[v => !!v || 'Patient Gender is required']" required />
                    <v-date-input  v-model="patientStore.patient.birthDate" clearable label="Birth of Date" :rules="[v => !!v || 'Patient Birth of Date is required']"
                        prepend-icon="" append-inner-icon="$calendar" :max="maxDate" :min="minDate"></v-date-input>
                    <v-container class="mt-6">
                        <v-row no-gutters justify="start">
                            <v-col cols="12" md="3">
                                <v-btn type="submit" color="success" rounded="xl" class="mb-3">
                                    Create
                                </v-btn>
                            </v-col>
                            <v-col cols="12" md="4">
                                <v-btn color="error" rounded="xl" class="mb-3" @click="resetForm">
                                    Reset Form
                                </v-btn>
                            </v-col>
                            <v-col cols="12" md="5">
                                <v-btn color="primary" rounded="xl" class="mb-3" @click="resetValidation">
                                    Reset Validation
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-container>
                    <v-overlay :model-value="isSubmitting" class="align-center justify-center">
                        <v-progress-circular :rotate="360" :size="100" :width="10" color="purple" indeterminate>
                        </v-progress-circular>
                    </v-overlay>
                </v-form>
            </v-card-text>
        </v-card>

        <v-dialog v-model="showSuccessDialog" block>
            <v-card>
                <v-card-title>
                    Success
                </v-card-title>
                <v-card-text>
                    Patient Create Successfully!
                </v-card-text>
                <v-card-actions>
                    <v-btn color="primary" @click.native="closeSuccessDialog">
                        OK
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

    </v-container>
    <v-container class="text-center ma-2">
        <v-card>
            <v-snackbar v-model="showErrorSnackbar" color="purple" variant="outlined">
                {{ errorMessage }}
                <template v-slot:actions>
                    <v-btn color="white" variant="outlined" @click="showErrorSnackbar = false">
                        Close
                    </v-btn>
                </template>
            </v-snackbar>
        </v-card>
    </v-container>
</template>

<style scoped>
.v-progress-circular {
    margin: 1rem;
}
</style>
