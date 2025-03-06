<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps({
    showModal: {
        type: Boolean,
    },
    
    selectedFamilyName:{
        type:String,
    },

    selectedGivenName:{
        type:String,  
    }
})

const patientName = computed(() =>{
    return props.selectedFamilyName + ', ' + props.selectedGivenName
})
</script>

<template>
        <!-- Delete Patient Modal -->
        <v-dialog max-width="45%" height="20%" v-model="props.showModal">
            <v-card>
                <v-card-title>
                    Confirm Patient Deletion
                </v-card-title>
                <v-card-text>
                    Are you sure you want to delete <strong>{{ patientName }}</strong>
                </v-card-text>
                <v-card-actions>
                    <v-btn color="grey" @click="$emit('cancelDelete')">
                        Cancel
                    </v-btn>
                    <v-btn color="red" @click="$emit('confirmDelete')" >
                        Confirm
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
</template>

<style scoped></style>
