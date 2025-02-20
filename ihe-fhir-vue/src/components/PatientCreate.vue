<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import { usePatientStore } from '@/stores/patientStore'

const router = useRouter()
const patientStore = usePatientStore()
const items = ref(['Male', 'Female', 'Unknown', 'Other'])
const patientForm = ref()
const isSubmitting = ref(false)
const showSuccessDialog = ref(false)
const showErrorSnackbar = ref(false)
const errorMessage = ref("")

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
    <v-container>
        <v-card>
            <v-card-title class="my-6">
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
                    <v-container class="mt-6">
                        <v-row no-gutters justify="center">
                            <v-col cols="12" md="1">
                                <v-btn :loading="isSubmitting" type="submit" color="success" rounded="xl" class="mb-3">
                                    Create
                                </v-btn>
                            </v-col>
                            <v-col cols="12" md="3">
                                <v-btn color="error" rounded="xl" class="mb-3"  @click="resetForm">
                                    Reset Form
                                </v-btn>
                            </v-col>
                            <v-col cols="12" md="3">
                                <v-btn color="primary" rounded="xl" class="mb-3"  @click="resetValidation">
                                    Reset Validation
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-container>
                </v-form>
            </v-card-text>
        </v-card>

        <v-dialog v-model="showSuccessDialog" max-width="400">
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
        <v-snackbar v-model="showErrorSnackbar" color="red" timeout="3000">
            {{ errorMessage }}
            <template v-slot:actions>
                <v-btn color="white" @click.native="showErrorSnackbar = false">
                    Close
                </v-btn>
            </template>
        </v-snackbar>
    </v-container>
</template>

<style scoped></style>
