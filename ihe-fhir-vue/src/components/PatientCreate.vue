<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import { usePatientStore } from '@/stores/patientStore'

const router = useRouter()
const patientStore = usePatientStore()

const isSubmitting = ref(false)

const submitForm = async() => {
    isSubmitting.value = true;
    
    let response =  await patientService.post(patientStore.patient)
    
    if (response.status === 200) {
        alert("created a new patient")
    } else{
        alert("failed to create a new patient")
    }
}

</script>

<template>
    <v-container>
        <v-card>
            <v-card-title>
                Create Patient
            </v-card-title>
            <v-card-text>
                <v-form @submit.prevent="submitForm">
                    <v-text-field v-model="patientStore.patient.familyName" label="Last Name"/>
                    <v-text-field v-model="patientStore.patient.givenName" label="First Name"/>
                    <v-select v-model="patientStore.patient.gender" label="Gender" :items="['Male', 'Female','Unknown', 'Other']"/>
                    <v-btn :loading = "isSubmitting" type="submit" color="primary"  rounded="xl">
                        Create
                    </v-btn>
                </v-form>
            </v-card-text>
        </v-card>

        <!-- <v-dialog v-model="showSuccessDialog" max-width="400">
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
        </v-snackbar> -->
    </v-container>
</template>

<style scoped></style>
