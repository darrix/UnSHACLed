/**
 * Provides basic DAO functionality (i.e. basic access to a persistence system).
 */
import {ModelComponent} from "../entities/modelTaskMetadata";

/**
 * An abstraction of database access;
 * can take on various forms depending on the actual database being used.
 */
export interface DataAccessObject {
    /**
     * Load the contents of the module from the persistence system
     * (i.e. database, filesystem, CVS, ...).
     * @param {Module} module
     */
    find(module: Module): void;

    /**
     * Load the workspace from the persistence system.
     */
    findWorkspace(module:Module): void;

    /**
     * Insert the contents of the module into the persistence system
     * (i.e. database, filesystem, CVS, ...).
     * @param {Module} module
     */
    insert(module: Module): void;

    /**
     * Insert workspace into the persistence system.
     */
    insertWorkspace(module: Module): void;
}

/**
 * A single persistence directive.
 * Contains all the necessary information to carry out a persistence operation.
 */
export interface Module {
    /**
     * Return the designated ModelComponent (used to identify a Component).
     */
    getTarget(): ModelComponent;

    /**
     * Return the identifier (used to identify a specific part of a Component).
     */
    getIdentifier(): string;

    /**
     * Return the content of the Module.
     */
    getContent(): any;

    /**
     * Returns the MIME type
     */
    getMime(): string;
}