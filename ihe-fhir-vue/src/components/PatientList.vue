<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import { usePatientStore } from '@/stores/patientStore'
import Patient from '@/models/Patient'

const router = useRouter()
const displayedPatients = ref([])
const retrievedPatients = ref<Patient[]>([])
const errorMessage = ref('')
const filteredPatients = ref<Patient[]>([])
const hasPatients = ref(false)
const isLoading = ref(true)

// selected patient to for deletion or viewing
const selectedPatient = ref<Patient | null>(null)
const deleteDialog = ref(false)

// show the delete confirmation modal
const confirmDelete = (patient: Patient) => {
    selectedPatient.value = patient
    deleteDialog.value = true
}

// handle delete action
const deletePatient = async () => {

    isLoading.value = true
    // remove the selected patient from db
    const response = await patientService.delete(selectedPatient.value?.id)
    if (response.status === 200) {
        isLoading.value = false
        filteredPatients.value = displayedPatients.value.filter((p: any) =>
            p.id != selectedPatient?.value?.id
        )
        displayedPatients.value = filteredPatients.value
        deleteDialog.value = false
        selectedPatient.value = null
        if(!displayedPatients.value){
            hasPatients.value = false
        }
    } else {
        isLoading.value = false
        router.push({ name: 'NotFound' })
    }
}

const headers = [
    { title: "Last Name", align: "left", key: "familyName" },
    { title: "First Name", align: "left", key: "givenName" },
    { title: "Gender", align: "left", key: "gender" },
    { title: "Birth Date", align: "left", key: "birthDate" },
    { title: "Actions", align: "left", key: "actions" },
];

onMounted(async () => {
    const response = await patientService.query()
    if (response.status === 200) {
        isLoading.value = false
        hasPatients.value = true
        retrievedPatients.value = response.data
        displayedPatients.value = retrievedPatients.value.map(({ id, familyName, givenName, gender, birthDate }) => ({
            id,
            familyName,
            givenName,
            gender,
            birthDate,
            actions: ''
        }))
    } else {
        isLoading.value = false
        hasPatients.value = false
    }
})

const editPatient = (patient: any) => {
    console.log("editing a patient: ", patient)
}

const createPatient = () => {
    router.push({name:"PatientCreate"})
}

</script>

<template>
    <v-overlay :model-value="isLoading" class="align-center justify-center">
        <v-progress-circular :rotate="360" :size="100" :width="10" color="purple" indeterminate>
        </v-progress-circular>
    </v-overlay>

    <v-container v-if="displayedPatients.length > 0">
        <v-data-table :headers="headers" :items="displayedPatients">
            <template v-slot:[`item.familyName`]="{ item }">
                <td class="text-left">{{ item.familyName }}</td> <!-- Left align for name -->
            </template>
            <template v-slot:[`item.givenName`]="{ item }">
                <td class="text-left">{{ item.givenName }}</td> <!-- Center align for age -->
            </template>
            <template v-slot:[`item.gender`]="{ item }">
                <td class="text-left">{{ item.gender }}</td> <!-- Right align for gender -->
            </template>
            <template v-slot:[`item.birthDate`]="{ item }">
                <td class="text-left">{{ item.birthDate }}</td> <!-- Right align for gender -->
            </template>
            <template v-slot:[`item.actions`]="{ item }">
                <td class="text-left">
                    <v-btn color="primary" @click="editPatient(item)">
                        <v-icon>mdi-pencil</v-icon>
                    </v-btn>
                    <v-btn color="red" class="ma-2" @click="confirmDelete(item)">
                        <v-icon>mdi-delete</v-icon>
                    </v-btn>
                </td>
            </template>
        </v-data-table>

        <!-- Delete  Confirmation Modal -->
        <v-dialog v-model="deleteDialog" max-width="45%" height="20%">
            <v-card>
                <v-card-title>
                    Confirm Patient Deletion
                </v-card-title>
                <v-card-text>
                    Are you sure you want to delete <strong>{{ `${selectedPatient?.familyName}
                        ${selectedPatient?.givenName}`
                        }}</strong>
                </v-card-text>
                <v-card-actions>
                    <v-btn color="grey" @click="deleteDialog = false">
                        Cancel
                    </v-btn>
                    <v-btn color="red" @click="deletePatient">
                        Confirm
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-container>
    <v-container v-else>
        <v-alert border="top" type="warning" variant="outlined" prominent>
            No patients found. Please create a new patient below to get started.
        </v-alert>
        <br/>
        <v-btn @click="createPatient" color="success" rounded="xl" variant="elevated">
            Create
        </v-btn>
    </v-container>
</template>

<style scoped></style>
