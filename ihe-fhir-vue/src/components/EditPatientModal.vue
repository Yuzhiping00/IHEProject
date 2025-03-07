<script setup lang="ts">
import { ref, nextTick, watch } from 'vue';
import { VDateInput } from 'vuetify/lib/labs/components.mjs'

const maxDate = ref(new Date())
const minDate = "1900-01-01"
const items = ref(['Male', 'Female', 'Unknown', 'Other'])
const form = ref()

const props = defineProps({
    showModal: Boolean,
    patient: Object,
})

const emit = defineEmits(["update-patient", "cancel-update"])
const loading = ref(false)
const formData = ref({ ...props.patient })

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

watch(() => props.patient,
    (newPatient) => {
        formData.value = { ...newPatient };
    }, { deep: true });

const savePatient = async() => {
    if(!form.value) {
        await nextTick()
    }

    if(!form.value) {
        console.error("Form reference is null")
        return
    }
    loading.value = true
    const { valid } = await form.value.validate()
    loading.value = false
    if (!valid) return
    emit("update-patient", formData.value)  
}

const cancelUpdate = () => {
    emit("cancel-update")
}
</script>

<template>
    <!-- Delete Patient Modal -->
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
                    <v-text-field label="Last Name" v-model="formData.familyName" :rules="firstNameRules"
                        required />
                    <v-text-field label="First Name" v-model="formData.givenName" :rules="lastNameRules"
                        required />
                    <v-select label="Gender" v-model="formData.gender" :items="items"
                        :rules="[v => !!v || 'Patient Gender is required']" required />
                    <v-date-input clearable label="Birth of Date" v-model="formData.birthDate"
                        :rules="[v => !!v || 'Patient Birth of Date is required']" prepend-icon=""
                        append-inner-icon="$calendar" :max="maxDate" :min="minDate"></v-date-input>
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
