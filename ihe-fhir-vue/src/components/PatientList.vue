<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from "vue-router"
import patientService from "@/services/resources/patientService.js"
import Patient from '@/models/Patient'
import DeletePatientModal from './DeletePatientModal.vue'
import EditPatientModal from './EditPatientModal.vue'


const router = useRouter()
const displayedPatients = ref<any[]>()
const retrievedPatients = ref<Patient[]>([])
const filteredPatients = ref<Patient[]>([])
const isLoading = ref(true)
const editPatient = ref()

// selected patient to for deletion or viewing
const selectedPatient = ref<Patient | null>(null)
const deleteDialog = ref(false)
const editDialog = ref(false)
const search = ref('')


const headers = [
    { title: "Last Name", align: "start", key: "familyName" },
    { title: "First Name", align: "start", key: "givenName" },
    { title: "Gender", align: "start", key: "gender" },
    { title: "Birth Date", align: "start", key: "birthDate" },
    { title: "Actions", align: "start", key: "actions" },
];

onMounted(async () => {
    isLoading.value = true
    const response = await patientService.query()
    isLoading.value = false
    if (response.status === 200) {
        isLoading.value = false
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
    }
})

// show the delete confirmation modal
const clickedDelete = (patient: Patient) => {
    selectedPatient.value = patient
    deleteDialog.value = true
}

// handle delete action
const confirmDeletePatient = async () => {
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
    } else {
        isLoading.value = false
        router.push({ name: 'NotFound' })
    }
}

const clickedEdit = (patient: any) => {
    editDialog.value = true
    editPatient.value = patient
    editPatient.value.birthDate = new Date(patient.birthDate)
    console.log("patient to be edited: ", editPatient.value.birthDate)
}

const handleUpdate = async (updatedPatient: any) => {
    let mappedPatient = retrievedPatients.value.find(p => p.id === updatedPatient.id)

    if (mappedPatient && updatedPatient) {
        Object.assign(mappedPatient, {
            birthDate: updatedPatient.birthDate,
            familyName: updatedPatient.familyName,
            givenName: updatedPatient.givenName,
            id: updatedPatient.id,
            gender: updatedPatient.gender,
        })
    }

    isLoading.value = true
    const response = await patientService.put(updatedPatient.id, mappedPatient)
    isLoading.value = false

    if (response.status === 200) {
        displayedPatients.value[displayedPatients.value.findIndex(p => p.id === updatedPatient.id)] = updatedPatient
        selectedPatient.value = null
        editDialog.value = false
    } else {
        router.push({ name: 'NotFound' })
    }
}

const cancelUpdateForm = () => {
    editDialog.value = false
}

const createPatient = () => {
    router.push({ name: "PatientCreate" })
}

</script>

<template>
    <v-overlay :model-value="isLoading" class="align-center justify-center">
        <v-progress-circular :rotate="360" :size="100" :width="10" color="purple" indeterminate>
        </v-progress-circular>
    </v-overlay>

    <v-container v-if="displayedPatients && displayedPatients.length > 0">
        <v-card title="Patients" flat class="text-left">
            <!-- search patients -->
            <template v-slot:text>
                <v-text-field v-model="search" label="Search" prepend-inner-icon="mdi-magnify" class="mt-5"
                    variant="outlined" hide-details single-line></v-text-field>
            </template>
            <v-data-table :headers="headers" :items="displayedPatients" :search="search">
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
                    <td class="text-left">{{ new Date(item.birthDate).toISOString().split("T")[0] }}</td>
                    <!-- Right align for gender -->
                </template>
                <template v-slot:[`item.actions`]="{ item }">
                    <td class="text-left">
                        <v-btn color="primary" @click="clickedEdit(item)">
                            <v-icon>mdi-pencil</v-icon>
                        </v-btn>
                        <v-btn color="red" class="ma-2" @click="clickedDelete(item)">
                            <v-icon>mdi-delete</v-icon>
                        </v-btn>
                    </td>
                </template>
            </v-data-table>
        </v-card>

        <!-- Delete  Confirmation Modal -->
        <delete-patient-modal v-if="deleteDialog" :show-modal="deleteDialog" @cancelDelete="deleteDialog = false"
            @confirmDelete="confirmDeletePatient" :selectedFamilyName="selectedPatient?.familyName"
            :selectedGivenName="selectedPatient?.givenName" />

        <!-- Edit Patient Modal -->
        <edit-patient-modal v-if="editDialog" :show-modal="editDialog" :patient="editPatient"
            @cancel-update="cancelUpdateForm" @update-patient="handleUpdate" />

    </v-container>

    <v-container v-else>
        <v-card>
            <v-alert type="warning" variant="outlined" prominent>
                <p class="font-weight-bold">No patients found. Please create a new patient below to get started.</p>
            </v-alert>
            <br />
            <v-btn @click="createPatient" color="success" rounded="xl" variant="elevated" class="mb-6">
                Create
            </v-btn>
        </v-card>

    </v-container>
</template>

<style scoped></style>
